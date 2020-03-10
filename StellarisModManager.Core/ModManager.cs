using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PDXModLib.ModData;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Stellaris.Data.Json;
using Stellaris.Data.Parser;

namespace StellarisModManager.Core
{
    public sealed class ModManager: IDisposable
    {
        private readonly Logger _logger;

        public ObservableCollection<ModEntry> Mods { get; }
        public string BasePath { get; }
        public string ModPath { get; }
        private readonly List<ModConflict> _conflicts;
        public IReadOnlyList<ModConflict> Conflicts => this._conflicts;

        public ModManager(IParser parser = null)
        {
            this._logger = new LoggerConfiguration()//
#if DEBUG
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
#else
                .MinimumLevel.Information()//
#endif
                .Enrich.FromLogContext()//
                .WriteTo.File("mod_manager.log")//
                .CreateLogger();//

            this._conflicts = new List<ModConflict>();
            this.Mods = new ObservableCollection<ModEntry>();
            this.BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            this.BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            this.ModPath = Path.Combine(this.BasePath, "mod");
            if(parser == null) parser = new SimpleModParser();
            var mods = new List<Mod>();
            foreach (var file in Directory.EnumerateFiles(this.ModPath, "*.mod"))
            {
                //var parsed = parser.Parse(file);
                //var mod = new Mod(parsed);
                //var mod = new Mod(file);
                //mod.Validate(file);
                var mod = Mod.Load(file);
                mods.Add(mod);
            }

            var gameData = LoadJson(Path.Combine(this.BasePath, "game_data.json"), x => {}, () => new GameData { ModsOrder = new List<string>() });
            var dlcLoad = LoadJson(Path.Combine(this.BasePath, "dlc_load.json"), x => {}, () => new DlcLoad {DisabledDlcs = new List<string>(), EnabledMods = new List<string>()});
            var modsRegistry = LoadJson(Path.Combine(this.BasePath, "mods_registry.json"), x => {}, () => new ModsRegistry());
            foreach (var t in gameData.ModsOrder)
            {
                if (!modsRegistry.TryGetValue(t, out var found)) continue;
                var alsoFound = mods.FirstOrDefault(x => x.Matches(found));
                if (alsoFound == null) continue;
                
                var it = new ModEntry(alsoFound, found.Id, found);
                it.IsEnabled = dlcLoad.EnabledMods.Any(x => x.Equals(it.ModData.Key, StringComparison.OrdinalIgnoreCase));
                this.Mods.Add(it);
            }

            foreach (var x1 in mods)
            {
                if (this.Mods.Any(y => y.ModData.Key == x1.Key)) continue;
                var found = modsRegistry.Where(x => x1.Matches(x.Value)).Select(x => x.Value).FirstOrDefault();
                var it = new ModEntry(x1, found?.Id, found);
                it.IsEnabled = dlcLoad.EnabledMods.Any(x => x.Equals(it.ModData.Key, StringComparison.OrdinalIgnoreCase));
                this.Mods.Add(it);
            }

            this.Validate();
            this.CalculateConficts();
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
            var gameData = LoadJson(gameDataFile, x => x.ModsOrder.Clear(), () => new GameData { ModsOrder = new List<string>() });
            var dlcLoadFile = Path.Combine(this.BasePath, "dlc_load.json");
            var dlcLoad = LoadJson(dlcLoadFile, x => x.EnabledMods.Clear(), () => new DlcLoad {DisabledDlcs = new List<string>(), EnabledMods = new List<string>()});
            var modsRegistryFile = Path.Combine(this.BasePath, "mods_registry.json");
            var modsRegistry = LoadJson(modsRegistryFile, x => x.Clear(), () => new ModsRegistry());

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
                    entry = new ModsRegistryEntry
                    {
                        Id = item.Guid.ToString(),
                        Source = !string.IsNullOrWhiteSpace(item.ModData.Folder)
                                 && (item.ModData.Folder.StartsWith(
                                         this.BasePath,
                                         StringComparison.OrdinalIgnoreCase)
                                     || item.ModData.Folder.StartsWith("mod/", StringComparison.OrdinalIgnoreCase))
                            ? SourceType.local
                            : SourceType.steam,
                        Status = item.ModData.Valid ? StatusType.ready_to_play : StatusType.invalid_mod,
                        SteamId = $"{item.ModData.RemoteFileId}",
                        DisplayName = item.ModData.Name,
                        GameRegistryId = item.ModData.Key,
                        RequiredVersion = item.ModData.SupportedVersion.ToString(),
                        Tags = new List<string>(item.ModData.Tags)
                    };
                    if (!string.IsNullOrWhiteSpace(item.ModData.FileName))
                    {
                        entry.ArchivePath = item.ModData.FileName;
                    }

                    if (!string.IsNullOrWhiteSpace(item.ModData.Folder)) entry.DirPath = item.ModData.Folder;
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
            this.CalculateConficts();
        }
        
        public void ReverseOrder()
        {
            for (var i = 0; i < this.Mods.Count; i++)
                this.Mods.Move(this.Mods.Count - 1, i);
            this.Validate();
            this.CalculateConficts();
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
            this.CalculateConficts();
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
            this.CalculateConficts();
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
            this.CalculateConficts();
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
            this.CalculateConficts();
        }

        public void CheckAll()
        {
            var all = !this.Mods.Any(x => x.IsSelected);

            foreach (var modEntry in this.Mods)
            {
                modEntry.IsEnabled = all || modEntry.IsSelected;
            }
            this.Validate();
            this.CalculateConficts();
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
            this.CalculateConficts();
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
            this.CalculateConficts();
        }

        public void Validate()
        {
            for(var i = 0; i < this.Mods.Count; i++)
            {
                var it = this.Mods[i];
                it.Issues.Clear();

                if (!it.IsEnabled) continue;
                if (it.ModData.Dependencies == null) continue;
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

        public void TopologicalSort()
        {
            try
            {
                var sorted1 = this.Mods.TopologicalSort(x => x.ModData.Dependencies?.Select(d => this.Mods.FirstOrDefault(y => string.Equals(y.Name, d, StringComparison.OrdinalIgnoreCase))).Where(m => m != null) ?? Enumerable.Empty<ModEntry>()).ToArray();
                this.Mods.Clear();
                foreach (var entry in sorted1) this.Mods.Add(entry);
                this.Validate();
                this.CalculateConficts();
            }
            catch (Exception e)
            {
                this._logger.Error(e, "TopologicalSort");
            }
        }

        public void CalculateConficts()
        {
            try 
            {
                this._conflicts.Clear();
                foreach (var entry in this.Mods)
                {
                    entry.Overwrites = false;
                    entry.IsOverwritten = false;
                }

                var enabled = this.Mods.Where(x => x.IsEnabled).ToArray();
                for (var i = 0; i < enabled.Length - 2; i++)
                {
                    var mod1 = enabled[i];
                    for(var j = i + 1; j < enabled.Length - 1; j++)
                    {
                        var mod2 = enabled[j];
                        this.AddModsConfict(mod1, mod2);
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.Error(e, "CalculateConficts");
            }
        }

        private void AddModsConfict(ModEntry mod1, ModEntry mod2)
        {
            try 
            {
                var modConflict = new ModConflict(mod1, mod2);
                foreach (var file in mod1.ModData.Files.Where(file => mod2.ModData.Files.Any(x => x.Path.Equals(file.Path, StringComparison.OrdinalIgnoreCase))))
                {
                    modConflict.AddConflictFile(file.Path);
                }

                if (modConflict.ConflictFiles.Count <= 0) return;
                mod1.IsOverwritten = true;
                mod2.Overwrites = true;
                this._conflicts.Add(modConflict);
            }
            catch (Exception e)
            {
                this._logger.Error(e, "AddModsConfict");
            }            
        }

        public void Dispose()
        {
            this._logger.Dispose();
        }
    }
}