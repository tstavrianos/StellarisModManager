using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;
using Paradox.Common.Models;

namespace Paradox.Common
{
    public sealed class ModManager
    {
        public ObservableCollection<ModData> Mods { get; }
        public string BasePath { get; }
        public string ModPath { get; }
        private readonly SupportedVersion _version;
        private readonly ILogger _logger;

        public ModManager(ILogger logger = null)
        {
            this._logger = logger;
            this._version = new SupportedVersion(2, 5, 1);
            this.Mods = new ObservableCollection<ModData>();
            this.BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            if (!Directory.Exists(this.BasePath))
                this.BasePath = @"C:\usefull\Newfolder\git\StellarisModManager\Stellaris";
            this.ModPath = Path.Combine(this.BasePath, "mod");

            var gameData = LoadJson(Path.Combine(this.BasePath, "game_data.json"), x => { }, () => new Json.GameData { ModsOrder = new List<string>() });
            var dlcLoad = LoadJson(Path.Combine(this.BasePath, "dlc_load.json"), x => { }, () => new Json.DlcLoad { DisabledDlcs = new List<string>(), EnabledMods = new List<string>() });
            var modsRegistry = LoadJson(Path.Combine(this.BasePath, "mods_registry.json"), x => { }, () => new Json.ModsRegistry());

            var mods = new List<ModData>();

            foreach (var (guid, entry) in modsRegistry)
            {
                if(string.IsNullOrWhiteSpace(entry.GameRegistryId)) continue;
                var modFile = Path.Combine(this.BasePath, entry.GameRegistryId);
                if (!File.Exists(modFile)) continue;
                var data = new ModData(modFile, entry, this._logger);
                data.Outdated = data.SupportedVersion < this._version;
                data.OriginalSpot = int.MaxValue;
                this.Mods.Add(data);
            }

            var i = 0;
            foreach (var t in gameData.ModsOrder)
            {
                var data = this.Mods.FirstOrDefault(x =>
                    x.Entry.Id.Equals(t, StringComparison.OrdinalIgnoreCase));
                if(data == null) continue;
                data.OriginalSpot = i++;
            }

            foreach (var t in dlcLoad.EnabledMods)
            {
                var data = this.Mods.FirstOrDefault(x =>
                    x.Key.Equals(t, StringComparison.OrdinalIgnoreCase));
                if(data == null) continue;
                data.IsChecked = true;
            }
            
            var items = this.Mods.OrderBy(x => x.OriginalSpot).ToArray();
            this.Mods.Clear();
            foreach (var item in items)
            {
                this.Mods.Add(item);
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

            foreach (var item in this.Mods)
                gameData.ModsOrder.Add(item.Entry.Id);

            foreach (var item in this.Mods.OrderBy(x => x.OriginalSpot))
                dlcLoad.EnabledMods.Add(item.Key);

            foreach (var item in this.Mods.OrderBy(x => x.OriginalSpot))
            {
                modsRegistry.Add(item.Entry.Id, item.Entry);
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
        }

        public void ReverseOrder()
        {
            for (var i = 0; i < this.Mods.Count; i++)
                this.Mods.Move(this.Mods.Count - 1, i);
        }

        public void MoveToTop(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex) return;
            this.Mods.Move(selectedIndex, 0);
        }

        public void MoveUp(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex) return;
            this.Mods.Move(selectedIndex, selectedIndex - 1);
        }

        public void MoveDown(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex + 1) return;
            this.Mods.Move(selectedIndex, selectedIndex + 1);
        }

        public void MoveToBottom(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex + 1) return;
            this.Mods.Move(selectedIndex, this.Mods.Count - 1);
        }

        public void CheckAll()
        {
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = true;
            }
        }

        public void UncheckAll()
        {
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = false;
            }
        }

        public void InvertCheck()
        {
            foreach (var modEntry in this.Mods)
            {
                modEntry.IsChecked = !modEntry.IsChecked;
            }
        }

        public void TopologicalSort()
        {
            try
            {
                var sorted1 = this.Mods.TopologicalSort(x => x.Dependencies?.Select(d => this.Mods.FirstOrDefault(y => string.Equals(y.Name, d, StringComparison.OrdinalIgnoreCase))).Where(m => m != null) ?? Enumerable.Empty<ModData>()).ToArray();
                this.Mods.Clear();
                foreach (var entry in sorted1) this.Mods.Add(entry);
            }
            catch (Exception e)
            {
                this._logger.Error(e, "TopologicalSort");
            }
        }
    }
}