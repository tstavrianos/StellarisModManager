using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Paradox.Common.Helpers;
using Splat;

namespace Paradox.Common
{
    public sealed class ModManager : IEnableLogger
    {
        public ObservableCollection<ModEntry> Mods { get; } = new ObservableCollection<ModEntry>();
        public ObservableCollection<ModEntry> Enabled { get; } = new ObservableCollection<ModEntry>();
        private readonly Dictionary<string, string> _fileConflicts;
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
                    //this.Log().Debug(d);
                    var found = this.Mods.FirstOrDefault(x => x.DisplayName == d);
                    if (found != null) mod.IdDependencies.Add(found.ModDefinitionFile.RemoteFileId);
                    else mod.NameDependencies.Add(d);
                }
            }
            this.Loaded = true;
            this._runValidation = true;
            this.Validate();
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
            //var modsRegistryFile = Path.Combine(this.BasePath, "mods_registry.json");
            //var modsRegistry = LoadJson(modsRegistryFile, x => x.Clear(), () => new Json.ModsRegistry());

            foreach (var item in this.Enabled)
                gameData.ModsOrder.Add(item.ModsRegistryEntry.Id);
            foreach (var item in this.Mods.Where(x => x.IsChecked == false).OrderBy(x => x.OriginalSpot))
                gameData.ModsOrder.Add(item.ModsRegistryEntry.Id);
            foreach (var item in this.Enabled.Reverse())
                dlcLoad.EnabledMods.Add(item.ModDefinitionFile.Key);
            /*foreach (var item in this.Mods.OrderBy(x => x.OriginalSpot))
                modsRegistry.Add(item.ModsRegistryEntry.Id, item.ModsRegistryEntry);*/

            BackupFile(gameDataFile);
            File.WriteAllText(gameDataFile, JsonConvert.SerializeObject(gameData));
            BackupFile(dlcLoadFile);
            File.WriteAllText(dlcLoadFile, JsonConvert.SerializeObject(dlcLoad));
            //BackupFile(modsRegistryFile);
            //File.WriteAllText(modsRegistryFile, JsonConvert.SerializeObject(modsRegistry));
        }

        public void AlphaSort()
        {
            this._runValidation = false;
            var items = this.Enabled.OrderBy(x => x.DisplayName).ToArray();
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
                    mod.ModDefinitionFile.ModifiedFiles.Any(y => y.Valid && y.FullPath == x.Key) &&
                    x.Value != mod.ModDefinitionFile.RemoteFileId);

                mod.OverwritesOthers = this._fileConflicts.Any(x =>
                    mod.ModDefinitionFile.ModifiedFiles.Any(y => y.Valid && y.FullPath == x.Key) &&
                                                                 x.Value == mod.ModDefinitionFile.RemoteFileId);

                mod.AllFilesOverwritten = mod.ModDefinitionFile.ModifiedFiles.All(x => x.Valid &&
                    this._fileConflicts.TryGetValue(x.FullPath, out var mod2) && mod2 != mod.ModDefinitionFile.RemoteFileId);
                if (!mod.AllFilesOverwritten) continue;
                mod.OverwritesOthers = false;
                mod.OverwrittenByOthers = false;
            }
        }

        private void AddModFileConflict(ModEntry mod1, ModEntry mod2)
        {
            foreach (var file in mod1.ModDefinitionFile.ModifiedFiles)
            {
                if (mod2.ModDefinitionFile.ModifiedFiles.Any(x => x.Valid && x.FullPath.Equals(file.FullPath, StringComparison.OrdinalIgnoreCase)))
                {
                    this._fileConflicts[file.FullPath] = mod2.ModDefinitionFile.RemoteFileId;
                }
            }
        }

        private static void SortAfterDependencies(IList<ModEntry> list, IEnumerable<string> dependencies, int order)
        {
            foreach (var n in dependencies)
            {
                var found = list.FirstOrDefault(x => x.DisplayName.Equals(n, StringComparison.OrdinalIgnoreCase));
                if(found == null) continue;
                var i = list.IndexOf(found);
                if (i <= order) continue;
                var item = list[order];
                list.RemoveAt(order);
                list.Insert(i, item);
                order = i;
            }
        }
        
        private static void SortAfterDependencies(ObservableCollection<ModEntry> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var order = i;
                var changed = false;
                var d = list[order].IdDependencies;
                foreach (var j in d)
                {
                    var found = list.FirstOrDefault(x => x.ModDefinitionFile.RemoteFileId == j);
                    if(found == null) continue;
                    var k = list.IndexOf(found);
                    if (k <= order) continue;
                    list.Move(order, k);
                    order = k;
                    changed = true;
                }
                d = list[order].NameDependencies;
                foreach (var j in d)
                {
                    var found = list.FirstOrDefault(x => x.ModDefinitionFile.Name == j);
                    if(found == null) continue;
                    var k = list.IndexOf(found);
                    if (k <= order) continue;
                    list.Move(order, k);
                    order = k;
                    changed = true;
                }

                if (changed) i--;
            }
        }

        private static Dictionary<string, IList<ModEntry>> MapTagsToDic(IEnumerable<ModEntry> list)
        {
            var ret = new Dictionary<string, IList<ModEntry>>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in list)
            {
                if(entry.ModDefinitionFile.Tags is null) continue;
                foreach (var tag in entry.ModDefinitionFile.Tags)
                {
                    if (!ret.TryGetValue(tag, out var l))
                    {
                        l = new List<ModEntry>();
                        ret[tag] = l;
                    }
                    l.Add(entry);
                }
            }
            
            return ret;
        }

        private static List<ModEntry> RemoveDupes(IEnumerable<ModEntry> list)
        {
            var ret = new List<ModEntry>();
            foreach (var l in list.Where(l => !ret.Contains(l)))
            {
                ret.Add(l);
            }

            return ret;
        }

        private static void Reorder(IList<ModEntry> list, ModEntry entry)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != entry) continue;
                list.RemoveAt(i);
                list.Add(entry);
                break;
            }
        }

        private static void InsertPair(IList<ModEntry> list, ModEntry a, ModEntry b)
        {
            ModEntry comp = null;
            for (var i = 0; i < list.Count; i++)
            {
                var mod = list[i];
                if (mod != b) continue;
                list.RemoveAt(i);
                comp = mod;
                break;
            }

            if (comp == null) return;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != a) continue;
                list.Insert(i + 1, comp);
                break;
            }
        }

        private static int PriorityForTag(string tag)
        {
            if (new[] {"OST", "Music", "Sound", "Graphics"}.Contains(tag, StringComparer.OrdinalIgnoreCase)) return 0;
            if (new[] {"AI", "Utilities", "Fixes"}.Contains(tag, StringComparer.OrdinalIgnoreCase)) return 1;
            if ("Patch".Equals(tag, StringComparison.OrdinalIgnoreCase)) return 2;
            return 3;
        }
        
        public void ExperimentalSort()
        {
            this._runValidation = false;

            var list = new ObservableCollection<ModEntry>(this.Enabled.OrderByDescending(x => x.DisplayName));
            for(var i = list.Count - 1; i > 0; i--)
            {
                var j = i - 1;
                if (list[j].DisplayName.StartsWith(list[i].DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    list.Swap(i, j);
                }
            }

            var output = new List<ModEntry>();
            var addAfter = new List<ModEntry>();
            var allTags = MapTagsToDic(list);
            foreach (var o in new[] {"OST", "Music", "Sound", "Graphics"}.Where(o => allTags.ContainsKey(o)))
            {
                output.AddRange(allTags[o]);
                allTags.Remove(o);
            }
            foreach (var o in new[] {"AI", "Utilities", "Fixes"}.Where(o => allTags.ContainsKey(o)))
            {
                addAfter.AddRange(allTags[o]);
                allTags.Remove(o);
            }

            if (allTags.ContainsKey("Patch"))
            {
                var l = allTags["Patch"];
                allTags.Remove("Patch");
                foreach (var x in l.Where(x => !addAfter.Contains(x)))
                {
                    addAfter.Add(x);
                }
            }

            foreach (var d in allTags)
            {
                switch (d.Value.Count)
                {
                    case 1:
                        break;
                    case 2:
                        InsertPair(list, d.Value[0], d.Value[1]);
                        break;
                    default:
                        output.AddRange(d.Value);
                        break;
                }
            }
            output.AddRange(addAfter);
            output = RemoveDupes(output);
            foreach (var entry in output)
            {
                Reorder(list, entry);
            }
            SortAfterDependencies(list);
            
            this.Enabled.Clear();
            foreach(var entry in list) this.Enabled.Add(entry);
            this._runValidation = true;
            this.Validate();
        }
    }
}