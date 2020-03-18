using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Splat;

namespace Paradox.Common
{
    public sealed class ModManager: IEnableLogger
    {
        public ObservableCollection<ModEntry> Mods { get; } = new ObservableCollection<ModEntry>();
        public ObservableCollection<ModEntry> Enabled { get; } = new ObservableCollection<ModEntry>();
        public string BasePath { get; }
        public string ModPath { get; }
        internal readonly SupportedVersion Version;
        internal bool Loaded;

        public ModManager()
        {
            this.Loaded = false;
            this.Version = new SupportedVersion(2, 5, 1);
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
                var modEntry = new ModEntry(this) { ModDefinitionFile = mod, ModsRegistryEntry = entry, OriginalSpot = i++, ModConflicts = new ObservableCollection<ModConflict>() };
                this.Mods.Add(modEntry);
            }

            foreach (var t in dlcLoad.EnabledMods)
            {
                var data = this.Mods.FirstOrDefault(x =>
                    x.ModDefinitionFile.Key.Equals(t, StringComparison.OrdinalIgnoreCase));
                if (data == null) continue;
                data.IsChecked = true;
                this.Enabled.Add(data);
            }

            foreach (var t in gameData.ModsOrder)
            {
                var data = this.Mods.FirstOrDefault(x =>
                    x.ModsRegistryEntry.Id.Equals(t, StringComparison.OrdinalIgnoreCase));
                if (data == null) continue;
                if (data.IsChecked == false) continue;
                this.Enabled.Add(data);
            }
            this.Loaded = true;
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
            var items = this.Enabled.OrderBy(x => x.ToString()).ToArray();
            this.Enabled.Clear();
            foreach (var item in items)
            {
                this.Enabled.Add(item);
            }
            this.Validate();
        }

        public void ReverseOrder()
        {
            for (var i = 0; i < this.Enabled.Count; i++)
                this.Enabled.Move(this.Enabled.Count - 1, i);
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
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = true;
            }
            this.Validate();
        }

        public void UncheckAll()
        {
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = false;
            }
            this.Validate();
        }

        public void InvertCheck()
        {
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = !modEntry.IsChecked;
            }
            this.Validate();
        }

        public void TopologicalSort()
        {
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
            this.Validate();
        }

        public void Validate()
        {
            foreach (var entry in this.Enabled)
            {
                entry.ModConflicts.Clear();
            }

            for (var i = 0; i < this.Enabled.Count; i++)
            {
                var modEntry = this.Enabled[i];
                foreach (var dependsOn in modEntry.ModDefinitionFile.Dependencies ?? Enumerable.Empty<string>())
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
                            found.ModConflicts.Add(new ModConflict { IsUp = true, DependsOn = modEntry.DisplayName });
                        }
                    }

                    if (!isDown && !isMissing) continue;
                    var conflict = new ModConflict
                    {
                        IsUp = false,
                        IsDown = isDown,
                        IsMissing = isMissing,
                        DependsOn = dependsOn
                    };
                    modEntry.ModConflicts.Add(conflict);
                }
            }
        }
    }
}