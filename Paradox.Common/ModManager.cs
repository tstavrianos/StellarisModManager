using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Paradox.Common.Helpers;
using Splat;

namespace Paradox.Common
{
    public sealed class ModManager : IEnableLogger
    {
        public ObservableCollection<ModEntry> Mods { get; } = new ObservableCollection<ModEntry>();
        public ObservableCollection<ModEntry> Enabled { get; } = new ObservableCollection<ModEntry>();
        private Dictionary<string, string> _fileConflicts;
        public string BasePath { get; }
        public string ModPath { get; }
        internal readonly SupportedVersion Version;
        internal readonly bool Loaded;

        private bool _runValidation;

        public ModManager()
        {
            this._runValidation = false;
            this._fileConflicts = new Dictionary<string, string>();
            this.Loaded = false;
            this.Version = new SupportedVersion(2, 6, 1);
            this.BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(this.BasePath))
                this.BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            this.ModPath = Path.Combine(this.BasePath, "mod");

            var gameData = LoadJson(Path.Combine(this.BasePath, "game_data.json"), x => { }, () => new Json.GameData { ModsOrder = new List<string>() });
            var dlcLoad = LoadJson(Path.Combine(this.BasePath, "dlc_load.json"), x => { }, () => new Json.DlcLoad { DisabledDlcs = new List<string>(), EnabledMods = new List<string>() });
            var modsRegistry = LoadJson(Path.Combine(this.BasePath, "mods_registry.json"), x => { }, () => new Json.ModsRegistry());
            var mods = ModDirectoryHelper.LoadModDefinitions(this.BasePath, true);
            var i = 0;
            foreach (var (guid, entry) in modsRegistry)
            {
                if (string.IsNullOrWhiteSpace(entry.GameRegistryId)) continue;
                var mod = mods.FirstOrDefault(x => x.Key.Equals(entry.GameRegistryId, StringComparison.OrdinalIgnoreCase));
                if (mod == null) continue;
                var modEntry = new ModEntry(this) { ModDefinitionFile = mod, ModsRegistryEntry = entry, OriginalSpot = i++, ModConflicts = new ObservableCollection<LoadOrderConflict>() };
                this.Mods.Add(modEntry);
            }

            foreach (var t in dlcLoad.EnabledMods)
            {
                var data = this.Mods.FirstOrDefault(x =>
                    x.ModDefinitionFile.Key.Equals(t, StringComparison.OrdinalIgnoreCase));
                if (data == null) continue;
                data.IsChecked = true;
            }

            foreach (var t in gameData.ModsOrder)
            {
                var data = this.Mods.FirstOrDefault(x =>
                    x.ModsRegistryEntry.Id.Equals(t, StringComparison.OrdinalIgnoreCase));
                if (data == null) continue;
                if (data.IsChecked == false) continue;
                this.Enabled.Add(data);
            }

            foreach (var mod in this.Mods)
            {
                mod.FillSupportedVersion();
                mod.IdDependencies = new ObservableHashSet<string>();
                mod.NameDependencies = new ObservableHashSet<string>();
                foreach (var d in mod.ModDefinitionFile.Dependencies ?? Enumerable.Empty<string>())
                {
                    var found = this.Mods.FirstOrDefault(x => x.DisplayName == d);
                    if (found != null) mod.IdDependencies.Add(found.ModDefinitionFile.RemoteFileId);
                    else mod.NameDependencies.Add(d);
                }
            }
            this.Loaded = true;
            this._runValidation = true;
            this.Validate();

            //Task.Run(Do).Wait();            
        }
        
        public class Tag
        {
            public string tag { get; set; }
        }

        public class Publishedfiledetail
        {
            public string publishedfileid { get; set; }
            public int result { get; set; }
            public string creator { get; set; }
            public int creator_app_id { get; set; }
            public int consumer_app_id { get; set; }
            public string filename { get; set; }
            public int file_size { get; set; }
            public string file_url { get; set; }
            public string hcontent_file { get; set; }
            public string preview_url { get; set; }
            public string hcontent_preview { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public int time_created { get; set; }
            public int time_updated { get; set; }
            public int visibility { get; set; }
            public bool banned { get; set; }
            public string ban_reason { get; set; }
            public int subscriptions { get; set; }
            public int favorited { get; set; }
            public int lifetime_subscriptions { get; set; }
            public int lifetime_favorited { get; set; }
            public int views { get; set; }
            public List<Tag> tags { get; set; }
        }

        public class Response
        {
            public int result { get; set; }
            public int resultcount { get; set; }
            public List<Publishedfiledetail> publishedfiledetails { get; set; }
        }

        public class GetPublishedFileDetailsResponse
        {
            public Response response { get; set; }
        }
        
        public static async Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(HttpClient webClient, string fileId) => await GetPublishedFileDetailsAsync(webClient, new List<string>() { fileId });

        public class Cache {
            public List<CacheFileDetail> FileDetails = new List<CacheFileDetail>();
        }
        public class CacheFileDetail : Publishedfiledetail {
            public DateTime lastFetched { get; set; }
            public static CacheFileDetail FromPublishedfiledetail(Publishedfiledetail publishedfiledetail)
            {
                var fdstr = JsonConvert.SerializeObject(publishedfiledetail);
                var res = JsonConvert.DeserializeObject<CacheFileDetail>(fdstr);
                res.lastFetched = DateTime.Now;
                return res;
            }
        }
        private static FileInfo cacheFile = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).CombineFile("steam.cache.json");
        private static Cache cache;
        private static void CheckCache()
        {
            if (cache is null) {
                if (cacheFile.Exists) {
                    try { cache = JsonConvert.DeserializeObject<Cache>(File.ReadAllText(cacheFile.FullName));
                    } catch (Exception ex) {
                        Console.WriteLine($"[ERROR] [Steam] Unable to load cache ({ex.Message}), starting over...");
                        cache = new Cache();
                    }
                } else {
                    cache = new Cache();
                }
            }
        }

        public static async Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(HttpClient webClient, List<string> fileIds)
        {
            var parsedResponse = new GetPublishedFileDetailsResponse();
            if (fileIds.Count < 1) return parsedResponse;
            CheckCache();
            if (cacheFile.Exists && (!cacheFile.LastWriteTime.ExpiredSince(10))) {
                foreach (var fileId in fileIds) {
                    var item = cache.FileDetails.FirstOrDefault(x => x.publishedfileid == fileId);
                    if (item != null) parsedResponse.response.publishedfiledetails.Add(item);
                }
                if (parsedResponse.response.publishedfiledetails.Count >= fileIds.Count)
                    return parsedResponse;
            }
            var values = new Dictionary<string, string> { { "itemcount", fileIds.Count.ToString() } };
            for (int i = 0; i < fileIds.Count; i++) {
                values.Add($"publishedfileids[{i}]", fileIds[i].ToString() );
            }
            var content = new FormUrlEncodedContent(values);
            var url = new Uri("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/");
            //Console.WriteLine($"[Steam] POST to {url} with payload {content.ToJson(false)} and values {values.ToJson(false)}");
            var response = await webClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            try { parsedResponse = JsonConvert.DeserializeObject<GetPublishedFileDetailsResponse>(responseString); }
            catch (Exception ex) { Console.WriteLine($"[Steam] Could not deserialize response ({ex.Message})\n{responseString}"); } // {response.ReasonPhrase} ({response.StatusCode})\n

            foreach (var item in parsedResponse.response.publishedfiledetails) {
                cache.FileDetails.RemoveAll(x => x.publishedfileid == item.publishedfileid);
                cache.FileDetails.Add(CacheFileDetail.FromPublishedfiledetail(item));
            }
            File.WriteAllText(cacheFile.FullName, JsonConvert.SerializeObject(cache));
            return parsedResponse;
        }

        private async Task Do()
        {
            var client = new HttpClient();
            
            var ids = this.Mods.Where(x => !string.IsNullOrWhiteSpace(x.ModDefinitionFile?.RemoteFileId) && ulong.TryParse(x.ModDefinitionFile.RemoteFileId, out var _)).Select(x => x.ModDefinitionFile.RemoteFileId).ToList();
            var t = await GetPublishedFileDetailsAsync(client, ids);
            foreach (var t1 in t.response.publishedfiledetails)
            {
                this.Log().Debug(t1.title);
            }
        }

        private static T LoadJson<T>(string file, Action<T> found, Func<T> notFound)
        {
            T ret;
            if (File.Exists(file))
            {
                ret =
                    JsonConvert.DeserializeObject<T>(
                        File.ReadAllText(file));
                found.Invoke(ret);
            }
            else
            {
                ret = notFound.Invoke();
            }

            return ret;
        }

        private static void BackupFile(string file)
        {
            if (!File.Exists(file)) return;
            var ext = Path.GetExtension(file);
            var i = 0;
            var newFile = Path.ChangeExtension(file, ext + $".{i:D3}");
            while (File.Exists(newFile))
            {
                i++;
                newFile = Path.ChangeExtension(file, ext + $".{i:D3}");
            }

            if (i <= 999)
            {
                File.Move(file, newFile);
            }
        }

        public void Save()
        {
            var gameDataFile = Path.Combine(this.BasePath, "game_data.json");
            var gameData = LoadJson(gameDataFile, x => x.ModsOrder.Clear(), () => new Json.GameData { ModsOrder = new List<string>() });
            var dlcLoadFile = Path.Combine(this.BasePath, "dlc_load.json");
            var dlcLoad = LoadJson(dlcLoadFile, x => x.EnabledMods.Clear(), () => new Json.DlcLoad { DisabledDlcs = new List<string>(), EnabledMods = new List<string>() });
            var modsRegistryFile = Path.Combine(this.BasePath, "mods_registry.json");
            var modsRegistry = LoadJson(modsRegistryFile, x => x.Clear(), () => new Json.ModsRegistry());

            foreach (var item in this.Enabled)
                gameData.ModsOrder.Add(item.ModsRegistryEntry.Id);
            foreach (var item in this.Mods.Where(x => x.IsChecked == false).OrderBy(x => x.OriginalSpot))
                gameData.ModsOrder.Add(item.ModsRegistryEntry.Id);
            foreach (var item in this.Mods.Where(x => x.IsChecked).OrderBy(x => x.OriginalSpot))
                dlcLoad.EnabledMods.Add(item.ModDefinitionFile.Key);
            foreach (var item in this.Mods.OrderBy(x => x.OriginalSpot))
                modsRegistry.Add(item.ModsRegistryEntry.Id, item.ModsRegistryEntry);

            BackupFile(gameDataFile);
            File.WriteAllText(gameDataFile, JsonConvert.SerializeObject(gameData));
            BackupFile(dlcLoadFile);
            File.WriteAllText(dlcLoadFile, JsonConvert.SerializeObject(dlcLoad));
            BackupFile(modsRegistryFile);
            File.WriteAllText(modsRegistryFile, JsonConvert.SerializeObject(modsRegistry));
        }

        public void AlphaSort()
        {
            this._runValidation = false;
            var items = this.Enabled.OrderBy(x => x.ToString()).ToArray();
            this.Enabled.Clear();
            foreach (var item in items)
            {
                this.Enabled.Add(item);
            }
            this._runValidation = true;
            this.Validate();
        }

        public void ReverseOrder()
        {
            this._runValidation = false;
            for (var i = 0; i < this.Enabled.Count; i++)
                this.Enabled.Move(this.Enabled.Count - 1, i);
            this._runValidation = true;
            this.Validate();
        }

        public void MoveToTop(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Enabled.Count <= selectedIndex) return;
            this.Enabled.Move(selectedIndex, 0);
            this.Validate();
        }

        public void MoveUp(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Enabled.Count <= selectedIndex) return;
            this.Enabled.Move(selectedIndex, selectedIndex - 1);
            this.Validate();
        }

        public void MoveDown(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Enabled.Count <= selectedIndex + 1) return;
            this.Enabled.Move(selectedIndex, selectedIndex + 1);
            this.Validate();
        }

        public void MoveToBottom(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Enabled.Count <= selectedIndex + 1) return;
            this.Enabled.Move(selectedIndex, this.Enabled.Count - 1);
            this.Validate();
        }

        public void CheckAll()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = true;
            }
            this._runValidation = true;
            this.Validate();
        }

        public void UncheckAll()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = false;
            }
            this._runValidation = true;
            this.Validate();
        }

        public void InvertCheck()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = !modEntry.IsChecked;
            }
            this._runValidation = true;
            this.Validate();
        }

        public void TopologicalSort()
        {
            this._runValidation = false;
            try
            {
                var sorted1 = this.Enabled.TopologicalSort(x => x.ModDefinitionFile.Dependencies?.Select(d => this.Enabled.FirstOrDefault(y => string.Equals(y.DisplayName, d, StringComparison.OrdinalIgnoreCase))).Where(m => m != null) ?? Enumerable.Empty<ModEntry>()).ToArray();
                this.Enabled.Clear();
                foreach (var entry in sorted1) this.Enabled.Add(entry);
            }
            catch (Exception e)
            {
                this.Log().Error(e, "TopologicalSort");
            }
            this._runValidation = true;
            this.Validate();
        }

        public void Validate()
        {
            if (!this._runValidation) return;
            foreach (var entry in this.Enabled)
            {
                entry.ModConflicts.Clear();
            }

            for (var i = 0; i < this.Enabled.Count; i++)
            {
                var modEntry = this.Enabled[i];
                foreach (var dependsOn in modEntry.IdDependencies)
                {
                    var found = this.Enabled.FirstOrDefault(x => x.ModDefinitionFile.RemoteFileId == dependsOn);
                    var isDown = false;
                    var isMissing = found == null;
                    if (found != null)
                    {
                        var foundIdx = this.Enabled.IndexOf(found);
                        if (foundIdx > i)
                        {
                            isDown = true;
                            found.ModConflicts.Add(new LoadOrderConflict { IsUp = true, DependsOnId = modEntry.ModDefinitionFile.RemoteFileId, DependsOnName = modEntry.DisplayName });
                        }
                    }

                    if (!isDown && !isMissing) continue;
                    var conflict = new LoadOrderConflict
                    {
                        IsUp = false,
                        IsDown = isDown,
                        IsMissing = isMissing,
                        DependsOnId = dependsOn,
                        DependsOnName = found?.DisplayName
                    };
                    modEntry.ModConflicts.Add(conflict);
                }

                foreach (var dependsOn in modEntry.NameDependencies)
                {
                    var found = this.Enabled.FirstOrDefault(x => x.DisplayName == dependsOn);
                    var isDown = false;
                    var isMissing = found == null;
                    if (found != null)
                    {
                        var foundIdx = this.Enabled.IndexOf(found);
                        if (foundIdx > i)
                        {
                            isDown = true;
                            if (found.ModConflicts.All(x => x.DependsOnName != modEntry.DisplayName && x.DependsOnId != modEntry.ModDefinitionFile.RemoteFileId))
                                found.ModConflicts.Add(new LoadOrderConflict { IsUp = true, DependsOnId = modEntry.ModDefinitionFile.RemoteFileId, DependsOnName = modEntry.DisplayName });
                        }
                    }

                    if (!isDown && !isMissing) continue;
                    var conflict = new LoadOrderConflict
                    {
                        IsUp = false,
                        IsDown = isDown,
                        IsMissing = isMissing,
                        DependsOnId = found?.ModDefinitionFile.RemoteFileId,
                        DependsOnName = dependsOn
                    };
                    if (!modEntry.ModConflicts.Any(x => x.DependsOnName == conflict.DependsOnName || (!string.IsNullOrEmpty(conflict.DependsOnId) && x.DependsOnId != conflict.DependsOnId)))
                        modEntry.ModConflicts.Add(conflict);
                }
            }

            this.ComputeConflicts();
        }

        private void ComputeConflicts()
        {
            this._fileConflicts.Clear();

            for (var i = 0; i < this.Enabled.Count - 1; i++)
            {
                var mod1 = this.Enabled[i];
                for (var j = i + 1; j < this.Enabled.Count; j++)
                {
                    var mod2 = this.Enabled[j];
                    this.AddModFileConflict(mod1, mod2);
                }
            }

            foreach (var mod in this.Enabled)
            {
                mod.OverwrittenByOthers = this._fileConflicts.Any(x =>
                    mod.ModDefinitionFile.ModifiedFiles.Contains(x.Key) &&
                    x.Value != mod.ModDefinitionFile.RemoteFileId);

                mod.OverwritesOthers = this._fileConflicts.Any(x =>
                    mod.ModDefinitionFile.ModifiedFiles.Contains(x.Key) &&
                    x.Value == mod.ModDefinitionFile.RemoteFileId);

                mod.AllFilesOverwritten = mod.ModDefinitionFile.ModifiedFiles.All(x =>
                    this._fileConflicts.TryGetValue(x, out var mod2) && mod2 != mod.ModDefinitionFile.RemoteFileId);
                if (mod.AllFilesOverwritten)
                {
                    mod.OverwritesOthers = false;
                    mod.OverwrittenByOthers = false;
                }
            }
        }

        private void AddModFileConflict(ModEntry mod1, ModEntry mod2)
        {
            foreach (var file in mod1.ModDefinitionFile.ModifiedFiles)
            {
                if (mod2.ModDefinitionFile.ModifiedFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
                {
                    this._fileConflicts[file] = mod2.ModDefinitionFile.RemoteFileId;
                }
            }
        }
    }
}