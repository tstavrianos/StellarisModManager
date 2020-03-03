using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWTools.CSharp;
using CWTools.Parser;
using CWTools.Process;
using Serilog;
using StellarisModManager.PDXModLib.Interfaces;
using StellarisModManager.PDXModLib.ModData;
using StellarisModManager.PDXModLib.Utilities;
using static CWTools.Parser.Types;
using static CWTools.Process.CK2Process;
using Position = CWTools.Utilities.Position;

namespace StellarisModManager.PDXModLib.GameContext
{
     public class GameContext : IGameContext
    {
        private readonly ILogger _logger;

        private const string SavedSelectionKey = "CurrentlySaved";
        private const string SelectionsKey = "Selections";

        #region Private fields

        private readonly IGameConfiguration _gameConfiguration;
        private readonly INotificationService _notificationService;

        private ModSelection _currentlySaved;

        private readonly IInstalledModManager _installedModManager;

        private EventRoot _settingsRoot;

        #endregion Private fields

        #region Public properties

        public IEnumerable<Mod> Mods => this._installedModManager.Mods;

		private readonly List<ModSelection> _selections = new List<ModSelection>();
		public IEnumerable<ModSelection> Selections => this._selections;

        public ModSelection CurrentSelection { get; set; }

		#endregion Public properties

		static GameContext()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public GameContext(IGameConfiguration gameConfiguration, INotificationService notificationService, IInstalledModManager installedModManager, ILogger logger)
        {
	        this._logger = logger;
	        this._gameConfiguration = gameConfiguration;
	        this._notificationService = notificationService;
	        this._installedModManager = installedModManager;
        }

        #region Public methods

        public async Task<bool> InitializeAsync()
        {
            try
            {
	            this._logger.Debug($"Loading settings from {this._gameConfiguration.SettingsPath}");

				if (null == (this._settingsRoot = LoadGameSettings(this._gameConfiguration.SettingsPath)))
                {
	                this._logger.Debug("Settings not loading, attempting backup");

					if (File.Exists(this._gameConfiguration.BackupPath))
                    {
	                    this._logger.Debug("Backup exists, asking user.");

						if (await this._notificationService.RequestConfirmationAsync("Settings.txt corrupted, backup available, reload from backup?", "Error").ConfigureAwait(false))
                        {
	                        this._logger.Debug($"Loading backup from {this._gameConfiguration.BackupPath}.");

	                        this._settingsRoot = LoadGameSettings(this._gameConfiguration.BackupPath);
                        }
                        else
                        {
	                        this._logger.Debug("User declined, exiting.");

							return false;
                        }
                    }
                    // haven't managed to load from backup
                    if (this._settingsRoot == null)
                    {
	                    this._logger.Debug("Loading failed, notifying user and exiting");

						await this._notificationService.ShowMessageAsync(
							"Settings.txt corrupted - no backup available, please run stellaris to recreate default settings.", "Error").ConfigureAwait(false);
                        return false;
                    }

                    this.SaveSettings();

					// this is a backup of an incorrect file, we should delete it. 
					File.Delete(this._gameConfiguration.BackupPath);
                }

				this._installedModManager.Initialize();
				this.LoadSavedSelection();
            }
            catch (Exception ex)
            {
                await this._notificationService.ShowMessageAsync(ex.Message, "Error").ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public bool SaveSettings()
        {
            try
            {
	            this._logger.Debug("Saving settings");
	            this._currentlySaved = this.CurrentSelection;
	            this.SaveSelection();

				var mods = this._settingsRoot.Child("last_mods");

				if (mods == null)
				{
					this._logger.Debug("No mods selected previously, creating a default selection node");

					mods = new Node("last_mods");
					this._settingsRoot.AllChildren = this._settingsRoot.AllChildren.Concat( new[] { Child.NewNodeC(mods.Value) }).ToList();
				}
				
				mods.Value.AllChildren = this.CurrentSelection.Contents.Select(c => Child.NewLeafValueC(new LeafValue(Value.NewQString(c.Key), Position.range.Zero))).ToList();


                if (File.Exists(this._gameConfiguration.BackupPath))
                {
	                this._logger.Debug($"Deleting backup path at {this._gameConfiguration.BackupPath}");
					File.Delete(this._gameConfiguration.BackupPath);
                }

                this._logger.Debug($"Creating backup at {this._gameConfiguration.BackupPath}");
				File.Move(this._gameConfiguration.SettingsPath, this._gameConfiguration.BackupPath);

				var visitor = new PrintingVisitor();

				visitor.Visit(this._settingsRoot);

				this._logger.Debug($"Saving game selection at {this._gameConfiguration.SettingsPath}");

				File.WriteAllText(this._gameConfiguration.SettingsPath, visitor.Result);
            }
            catch (Exception ex)
            {
	            this._logger.Error(ex, "Error saving game settings");
                return false;
            }

            return true;
        }

        public bool SaveSelection()
        {
            try
            {
	            this._logger.Debug("Saving selections");

				var selectionsToSave = new Node("root");
				var selections = new Node(SelectionsKey);

				var savedSelection = Child.NewLeafC(new Leaf(SavedSelectionKey, Value.NewQString(this._currentlySaved?.Name), Position.range.Zero));

				selections.AllChildren = this.Selections.Select(s =>
				{

					var r = new Node($"\"{s.Name}\"")
					{
						AllChildren = s.Contents.Select(c =>
							Child.NewLeafValueC(new LeafValue(Value.NewQString(c.Key), Position.range.Zero))).ToList()
					};
					return Child.NewNodeC(r);
				}).ToList();

				selectionsToSave.AllChildren = 
					new[] { Child.NewNodeC(selections), savedSelection }.ToList();


				var visitor = new PrintingVisitor();

				visitor.Visit(selectionsToSave);

				this._logger.Debug($"Writing all selections to {this._gameConfiguration.SavedSelections}");

				File.WriteAllText(this._gameConfiguration.SavedSelections, visitor.Result);
            }
            catch (Exception ex)
            {
	            this._logger.Error(ex, "Error saving mod selection settings");
                return false;
            }

            return true;
        }

        public Task<bool> SaveMergedModAsync(MergedMod mod, bool mergedFilesOnly)
        {
            return this._installedModManager.SaveMergedModAsync(mod, mergedFilesOnly);
        }

        private void LoadSavedSelection()
        {
	        this._logger.Debug($"Attempting to load saved selections from {this._gameConfiguration.SavedSelections}");
            if (File.Exists(this._gameConfiguration.SavedSelections))
            {
	            this._logger.Debug("Settings file exists, parsing.");

				var adapter = CwToolsAdapter.Parse(this._gameConfiguration.SavedSelections);

				if (adapter.Root != null) 
				{
					this.UpgradeFormat(adapter.Root);

					var selectionIdx = adapter.Root.Leafs(SavedSelectionKey).FirstOrDefault().Value.ToRawString();

					this._logger.Debug($"Current selection was previously saved as {selectionIdx}");

					var selections = adapter.Root.Child(SelectionsKey).Value?.AllChildren ?? Enumerable.Empty<Child>();

                    foreach (var selection in selections.Where(s => s.IsNodeC).Select(s => s.node))
                    {
						var key = selection.Key.Trim('"');
                        ModSelection modSelection;
                        if (selection.Key.Equals(selectionIdx))
                        {
                            modSelection = this.CreateDefaultSelection(key);
                            this.CurrentSelection = modSelection;
                        }
                        else
                        {
                            modSelection = this.CreateFromScObject(key, selection.AllChildren);
                        }

                        this._selections.Add(modSelection);
                    }
                }
            }

            // only happens if the config file couldn't have been loaded
            if (this.CurrentSelection == null)
            {
	            this._logger.Debug("Settings file does not exist, creating default selection.");
	            this.CurrentSelection = this.CreateDefaultSelection();
	            this._selections.Add(this.CurrentSelection);
            }

            this._currentlySaved = this.CurrentSelection;
            this.SaveSelection();
        }

        public void LoadMods()
        {
	        this._installedModManager.LoadMods();
        }

        public void DeleteCurrentSelection()
        {
	        this._selections.Remove(this.CurrentSelection);
	        this.CurrentSelection = this.Selections.FirstOrDefault();
            if (!this.Selections.Contains(this._currentlySaved))
            {
	            this._currentlySaved = null;
            }

            this.SaveSelection();
        }

        public void DuplicateCurrentSelection(string newName)
        {
	        this._logger.Debug($"Duplicating current selection and naming it {newName}");
            var sel = new ModSelection(newName);
            sel.Contents.AddRange(this.CurrentSelection.Contents);
            this.CurrentSelection = sel;
            this._selections.Add(sel);
            this.SaveSelection();
        }

        #endregion Public methods

        #region Private methods

        private static EventRoot LoadGameSettings(string path)
        {
			var result = CKParser.parseEventFile(path);

			if (result.IsFailure)
				return null;

			var root = processEventFile(result.GetResult());

			return !root.All.Any() ? null : root;
        }

        private void UpgradeFormat(Node selectionsDocument)
        {
            //Upgrade from Stellaris only names; 
            var upgradeNeeded = selectionsDocument.Child("SavedToStellaris") != null;
            if (upgradeNeeded)
            {
	            this._logger.Debug("Upgrading selection from old Stellaris format");

	            var newNode = new Node(SavedSelectionKey)
	            {
		            AllChildren = selectionsDocument.Child("SavedToStellaris").Value.AllChildren
	            };

	            var replacements = selectionsDocument.AllChildren.Where(f => !(f.IsNodeC && f.node.Key == "SavedToStellaris")).ToList();

	            replacements.Add(Child.NewNodeC(newNode));

				selectionsDocument.AllChildren = replacements;
            }

			var upgradeSelectionKeys = selectionsDocument.Child(SelectionsKey).Value?.Nodes.All(c => c.Key.StartsWith("\"", StringComparison.Ordinal) && c.Key.EndsWith("\"", StringComparison.Ordinal)) ?? false;
			if (upgradeSelectionKeys)
			{
				this._logger.Debug("Upgrading selection from old parser format");
				var selections = selectionsDocument.Child(SelectionsKey).Value;
				var nodes = selections.Nodes.ToList();
				var newChildren = new List<Child>();
				foreach (var node in nodes)
				{
					var nn = new Node(node.Key.Trim('"'), Position.range.Zero) {AllChildren = node.AllChildren};
					newChildren.Add(Child.NewNodeC(nn));
				}
				selections.AllChildren = newChildren;
			}
			var ss2 = selectionsDocument.Child(SelectionsKey).Value;
		}

        private ModSelection CreateDefaultSelection(string name = "Default selection")
        {
            return this.CreateFromScObject(name, this._settingsRoot.Child("last_mods")?.Value.AllChildren ?? Enumerable.Empty<Child>());
        }

        private ModSelection CreateFromScObject(string name, IEnumerable<Child> contents)
        {
	        this._logger.Debug($"Creating selection named {name}");
            var selection = new ModSelection(name);

            foreach (var installed in contents.Where(c => c.IsLeafValueC).Select(c => c.lefavalue).Select(mod => this._installedModManager.Mods.FirstOrDefault(m => m.Key == mod.Value.ToRawString())).Where(installed => installed != null))
            {
	            selection.Contents.Add(installed);
            }
            return selection;
        }

        #endregion Private methods
    }
}