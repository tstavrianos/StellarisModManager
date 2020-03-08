using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Stellaris.Data;
using Stellaris.Data.Json;
using Stellaris.Data.Parser;

namespace StellarisModManager.Core
{
    public sealed class ModManager
    {
        public ObservableCollection<ModEntry> Mods { get; }
        public string BasePath { get; }
        public string ModPath { get; }

        public ModManager(IParser parser = null)
        {
            this.Mods = new ObservableCollection<ModEntry>();
            this.BasePath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            this.ModPath = Path.Combine(this.BasePath, "mod");
            if(parser == null) parser = new SimpleModParser();
            var mods = (from file in Directory.EnumerateFiles(this.ModPath, "*.mod") let table = parser.Parse(file) select new Mod(table, file)).ToList();

            var gameData = LoadJson(Path.Combine(this.BasePath, "game_data.json"), x => {}, () => new GameData { ModsOrder = new List<string>() });
            var dlcLoad = LoadJson(Path.Combine(this.BasePath, "dlc_load.json"), x => {}, () => new DlcLoad {DisabledDlcs = new List<string>(), EnabledMods = new List<string>()});
            var modsRegistry = LoadJson(Path.Combine(this.BasePath, "mods_registry.json"), x => {}, () => new ModsRegistry());
            foreach (var t in gameData.ModsOrder)
            {
                if (!modsRegistry.TryGetValue(t, out var found)) continue;
                var alsoFound = mods.FirstOrDefault(x => x.Key.Equals(found.GameRegistryId, StringComparison.OrdinalIgnoreCase));
                if (alsoFound == null) continue;
                
                var it = new ModEntry(alsoFound, found.Id, found);
                it.IsEnabled = dlcLoad.EnabledMods.Any(x => x.Equals(it.ModData.Key, StringComparison.OrdinalIgnoreCase));
                this.Mods.Add(it);
            }

            foreach (var it in from x1 in mods where this.Mods.All(y => y.ModData.Key != x1.Key) let found = modsRegistry.Where(x => x.Value.GameRegistryId.Equals(x1.Key, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).FirstOrDefault() select new ModEntry(x1, found?.Id, found))
            {
                it.IsEnabled = dlcLoad.EnabledMods.Any(x => x.Equals(it.ModData.Key, StringComparison.OrdinalIgnoreCase));
                this.Mods.Add(it);
            }
            
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
            var gameData = LoadJson<GameData>(gameDataFile, x => x.ModsOrder.Clear(), () => new GameData { ModsOrder = new List<string>() });
            var dlcLoadFile = Path.Combine(this.BasePath, "dlc_load.json");
            var dlcLoad = LoadJson<DlcLoad>(dlcLoadFile, x => x.EnabledMods.Clear(), () => new DlcLoad {DisabledDlcs = new List<string>(), EnabledMods = new List<string>()});
            var modsRegistryFile = Path.Combine(this.BasePath, "mods_registry.json");
            var modsRegistry = LoadJson<ModsRegistry>(modsRegistryFile, x => x.Clear(), () => new ModsRegistry());

            foreach(var item in this.Mods)
                gameData.ModsOrder.Add(item.Guid.ToString());
            
            foreach(var item in this.Mods.OrderBy(x => x.OriginalSpot))
                dlcLoad.EnabledMods.Add(item.ModData.Key);
            
            foreach (var item in this.Mods.OrderBy(x => x.OriginalSpot))
            {
                ModsRegistryEntry entry;
                if (item.RegistryData != null) entry = item.RegistryData;
                else
                {
                    entry = new ModsRegistryEntry();
                    entry.Id = item.Guid.ToString();
                    entry.Source = !string.IsNullOrWhiteSpace(item.ModData.Path)
                                   && (item.ModData.Path.StartsWith(
                                           this.BasePath,
                                           StringComparison.OrdinalIgnoreCase)
                                       || item.ModData.Path.StartsWith("mod/", StringComparison.OrdinalIgnoreCase))
                                       ? SourceType.local
                                       : SourceType.steam;
                    entry.Status = item.ModData.Valid ? StatusType.ready_to_play : StatusType.invalid_mod;
                    entry.SteamId = $"{item.ModData.RemoteFileId}";
                    entry.DisplayName = item.ModData.Name;
                    entry.GameRegistryId = item.ModData.Key;
                    entry.RequiredVersion =
                        item.ModData.SupportedVersion != null ? item.ModData.SupportedVersion.ToString() : "1.*";
                    entry.Tags = new List<string>(item.ModData.Tags);
                    if (!string.IsNullOrWhiteSpace(item.ModData.Archive))
                    {
                        entry.ArchivePath = item.ModData.Archive;
                    }

                    if (!string.IsNullOrWhiteSpace(item.ModData.Path)) entry.DirPath = item.ModData.Path;
                }

                modsRegistry.Add(entry.Id, entry);
            }

            BackupFile(gameDataFile);
            File.WriteAllText(gameDataFile, JsonConvert.SerializeObject(gameData));
            BackupFile(dlcLoadFile);
            File.WriteAllText(dlcLoadFile, JsonConvert.SerializeObject(dlcLoad));
            BackupFile(modsRegistryFile);
            File.WriteAllText(modsRegistryFile, JsonConvert.SerializeObject(modsRegistry));
        }
        
        public void AlphaSort()
        {
            var items = this.Mods.OrderBy(x => x.ToString()).ToArray();
            this.Mods.Clear();
            foreach (var item in items)
            {
                this.Mods.Add(item);
            }
            this.Validate();
        }
        
        public void ReverseOrder()
        {
            for (var i = 0; i < this.Mods.Count; i++)
                this.Mods.Move(this.Mods.Count - 1, i);
            this.Validate();
        }

        public void MoveToTop()
        {
            var last = -1;
            for (var i = 1; i < this.Mods.Count; i++)
            {
                if (!this.Mods[i].IsSelected) continue;
                this.Mods.Move(i, last + 1);
                last++;
            }
            this.Validate();
        }

        public void MoveUp()
        {
            for (var i = 1; i < this.Mods.Count; i++)
            {
                if (this.Mods[i].IsSelected && !this.Mods[i - 1].IsSelected)
                {
                    this.Mods.MoveItemUp(i);
                }
            }
            this.Validate();
        }

        public void MoveDown()
        {
            for (var i = this.Mods.Count - 2; i >= 0; i--)
            {
                if (this.Mods[i].IsSelected && !this.Mods[i + 1].IsSelected)
                {
                    this.Mods.MoveItemDown(i);
                }
            }
            this.Validate();
        }

        public void MoveToBottom()
        {
            var first = this.Mods.Count;
            for (var i = this.Mods.Count-2; i >= 0; i--)
            {
                if (!this.Mods[i].IsSelected) continue;
                this.Mods.Move(i, first - 1);
                first--;
            }
            this.Validate();
        }

        public void CheckAll()
        {
            var all = !this.Mods.Any(x => x.IsSelected);

            foreach (var modEntry in this.Mods)
            {
                modEntry.IsEnabled = all || modEntry.IsSelected;
            }
            this.Validate();
        }

        public void UncheckAll()
        {
            var all = !this.Mods.Any(x => x.IsSelected);

            foreach (var modEntry in this.Mods)
            {
                if (!all)
                {
                    if (modEntry.IsSelected) modEntry.IsEnabled = false;
                }
                else
                {
                    modEntry.IsEnabled = false;
                }
            }
            this.Validate();
        }

        public void InvertCheck()
        {
            var all = !this.Mods.Any(x => x.IsSelected);

            foreach (var modEntry in this.Mods)
            {
                if (!all)
                {
                    if (modEntry.IsSelected) modEntry.IsEnabled = !modEntry.IsEnabled;
                }
                else
                {
                    modEntry.IsEnabled = !modEntry.IsEnabled;
                }
            }        
            this.Validate();
        }

        public void Validate()
        {
            for(var i = 0; i < this.Mods.Count; i++)
            {
                var it = this.Mods[i];
                it.Issues.Clear();

                if (it.IsEnabled)
                {
                    if (it.ModData.Dependencies != null)
                    {
                        foreach (var dep in it.ModData.Dependencies)
                        {
                            var depMod = this.Mods.FirstOrDefault(x => x.Name == dep);
                            if (depMod == null)
                            {
                                it.Issues.Add($"{dep} is not installed");
                                continue;
                            }
                            var depModIdx = this.Mods.IndexOf(depMod);
                            if (!depMod.IsEnabled)
                            {
                                it.Issues.Add($"{dep} should be enabled");
                            }
                            if (depModIdx > i)
                            {
                                it.Issues.Add($"{dep} should be higher than {it.Name}");
                            } 
                            else if (depModIdx == i)
                            {
                                it.Issues.Add("Cannot depend on itself");
                            }
                        }
                    }
                }
            }

        }
    }
}