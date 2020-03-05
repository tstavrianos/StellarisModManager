using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Serilog;
using StellarisModManager.Configuration;
using StellarisModManager.Models;
using PDXModLib.GameContext;

namespace StellarisModManager
{
    using PDXModLib.ModData;

    internal sealed partial class MainWindow
    {
        private readonly StellarisConfiguration _configuration;
        public ObservableCollection<ItemPresenter> Test { get; set; } = new ObservableCollection<ItemPresenter>();

        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            this._configuration = new StellarisConfiguration();

            var manager = new InstalledModManager(this._configuration, null, logger);
            manager.Initialize();
            /*foreach (var m in manager.Mods)
            {
                this.Test.Add(new ItemPresenter(m));
            }*/

            var gameData = LoadJson<GameData>(Path.Combine(this._configuration.BasePath, "game_data.json"), x => {}, () => new GameData { ModsOrder = new List<string>() });
            var dlcLoad = LoadJson<DlcLoad>(Path.Combine(this._configuration.BasePath, "dlc_load.json"), x => {}, () => new DlcLoad {DisabledDlcs = new List<string>(), EnabledMods = new List<string>()});
            var modsRegistry = LoadJson<ModsRegistry>(Path.Combine(this._configuration.BasePath, "mods_registry.json"), x => {}, () => new ModsRegistry());
            foreach (var t in gameData.ModsOrder)
            {
                if (!modsRegistry.TryGetValue(t, out var found)) continue;
                var alsoFound = manager.Mods.FirstOrDefault(x => x.Key.Equals(found.GameRegistryId, StringComparison.OrdinalIgnoreCase));
                if (alsoFound == null) continue;
                var it = new ItemPresenter(alsoFound, found.Id);
                it.IsChecked = dlcLoad.EnabledMods.Any(x => x.Equals(it.Mod.Key, StringComparison.OrdinalIgnoreCase));
                it.Entry = found;
                this.Test.Add(it);
            }

            foreach (var mod in manager.Mods.Where(x => this.Test.All(y => y.Mod.Key != x.Key)))
            {
                var found = modsRegistry.Where(x => x.Value.GameRegistryId.Equals(mod.Key, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).FirstOrDefault();
                var it = found != null ? new ItemPresenter(mod, found.Id) : new ItemPresenter(mod);
                it.IsChecked = dlcLoad.EnabledMods.Any(x => x.Equals(it.Mod.Key, StringComparison.OrdinalIgnoreCase));
                it.Entry = found;
                this.Test.Add(it);
            }

            this.InitializeComponent();
            this.MoveToTop.IsEnabled = false;
            this.MoveUp.IsEnabled = false;
            this.MoveDown.IsEnabled = false;
            this.MoveToBottom.IsEnabled = false;
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


        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            var gameDataFile = Path.Combine(this._configuration.BasePath, "game_data.json");
            var gameData = LoadJson<GameData>(gameDataFile, x => x.ModsOrder.Clear(), () => new GameData { ModsOrder = new List<string>() });
            var dlcLoadFile = Path.Combine(this._configuration.BasePath, "dlc_load.json");
            var dlcLoad = LoadJson<DlcLoad>(dlcLoadFile, x => x.EnabledMods.Clear(), () => new DlcLoad {DisabledDlcs = new List<string>(), EnabledMods = new List<string>()});
            var modsRegistryFile = Path.Combine(this._configuration.BasePath, "mods_registry.json");
            var modsRegistry = LoadJson<ModsRegistry>(modsRegistryFile, x => x.Clear(), () => new ModsRegistry());

            foreach(var item in this.Test)
                gameData.ModsOrder.Add(item.Guid.ToString());
            
            foreach(var item in this.Test.OrderBy(x => x.OriginalSpot))
                dlcLoad.EnabledMods.Add(item.Mod.Key);
            
            foreach (var item in this.Test.OrderBy(x => x.OriginalSpot))
            {
                ModsRegistryEntry entry;
                if (item.Entry != null) entry = item.Entry;
                else
                {
                    entry = new ModsRegistryEntry();
                    entry.Id = item.Guid.ToString();
                    entry.Source = !string.IsNullOrWhiteSpace(item.Mod.Folder)
                                   && (item.Mod.Folder.StartsWith(
                                           this._configuration.BasePath,
                                           StringComparison.OrdinalIgnoreCase)
                                       || item.Mod.Folder.StartsWith("mod/", StringComparison.OrdinalIgnoreCase))
                                       ? SourceType.local
                                       : SourceType.steam;
                    entry.Status = item.Mod.Files != null && item.Mod.Files.Count > 0
                                       ? StatusType.ready_to_play
                                       : StatusType.invalid_mod;
                    entry.SteamId = item.Mod.RemoteFileId;
                    entry.DisplayName = item.Mod.Name;
                    entry.GameRegistryId = item.Mod.Key;
                    entry.RequiredVersion =
                        item.Mod.SupportedVersion != null ? item.Mod.SupportedVersion.ToString() : "1.*";
                    entry.Tags = new List<string>(item.Mod.Tags);
                    if (!string.IsNullOrWhiteSpace(item.Mod.FileName))
                    {
                        entry.ArchivePath = item.Mod.FileName;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Mod.Folder)) entry.DirPath = item.Mod.Folder;
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

        private void Alpha_Clicked(object sender, RoutedEventArgs e)
        {
            var items = this.Test.OrderBy(x => x.ToString()).ToArray();
            this.Test.Clear();
            foreach (var item in items)
            {
                this.Test.Add(item);
            }
        }
        
        private void Reverse_Clicked(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < this.Test.Count; i++)
                this.Test.Move(this.Test.Count - 1, i);
        }

        private void MoveToTop_Clicked(object sender, RoutedEventArgs e)
        {
            var last = -1;
            for (var i = 1; i < this.Test.Count; i++)
            {
                if (!this.Test[i].IsSelected) continue;
                this.Test.Move(i, last + 1);
                last++;
            }
            this.CheckButtons();
        }

        private void MoveUp_Clicked(object sender, RoutedEventArgs e)
        {
            for (var i = 1; i < this.Test.Count; i++)
            {
                if (this.Test[i].IsSelected && !this.Test[i - 1].IsSelected)
                {
                    this.Test.MoveItemUp(i);
                }
            }
            this.CheckButtons();
        }

        private void MoveDown_Clicked(object sender, RoutedEventArgs e)
        {
            for (var i = this.Test.Count - 2; i >= 0; i--)
            {
                if (this.Test[i].IsSelected && !this.Test[i + 1].IsSelected)
                {
                    this.Test.MoveItemDown(i);
                }
            }
            this.CheckButtons();
        }

        private void MoveToBottom_Clicked(object sender, RoutedEventArgs e)
        {
            var first = this.Test.Count;
            for (var i = this.Test.Count-2; i >= 0; i--)
            {
                if (!this.Test[i].IsSelected) continue;
                this.Test.Move(i, first - 1);
                first--;
            }
            this.CheckButtons();
        }

        private void ModList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CheckButtons();
        }

        private void CheckButtons()
        {
            var first = -1;
            var last = -1;
            for (var i = 0; i < this.Test.Count; i++)
            {
                if (!this.Test[i].IsSelected) continue;
                if (first == -1) first = i;
                last = i;
            }
            this.MoveToTop.IsEnabled = last > 0;
            this.MoveUp.IsEnabled = last > 0;
            this.MoveDown.IsEnabled = first >= 0 && first < this.Test.Count - 1;
            this.MoveToBottom.IsEnabled = first >= 0 && first < this.Test.Count - 1;
        }

        private void CheckAll_Clicked(object sender, RoutedEventArgs e)
        {
            var all = !this.Test.Any(x => x.IsSelected);

            foreach (var itemPresenter in this.Test)
            {
                itemPresenter.IsChecked = all || itemPresenter.IsSelected;
            }
        }

        private void UncheckAll_Clicked(object sender, RoutedEventArgs e)
        {
            var all = !this.Test.Any(x => x.IsSelected);

            foreach (var itemPresenter in this.Test)
            {
                if (!all)
                {
                    if (itemPresenter.IsSelected) itemPresenter.IsChecked = false;
                }
                else
                {
                    itemPresenter.IsChecked = false;
                }
            }
        }

        private void InvertCheck_Clicked(object sender, RoutedEventArgs e)
        {
            var all = !this.Test.Any(x => x.IsSelected);

            foreach (var itemPresenter in this.Test)
            {
                if (!all)
                {
                    if (itemPresenter.IsSelected) itemPresenter.IsChecked = !itemPresenter.IsChecked;
                }
                else
                {
                    itemPresenter.IsChecked = !itemPresenter.IsChecked;
                }
            }        
        }
    }
}