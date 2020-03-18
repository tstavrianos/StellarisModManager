using System.Text.RegularExpressions;
using Paradox.Common.Helpers;
using Paradox.Common.Json;
using ReactiveUI;
using Splat;
using System.Collections.ObjectModel;

namespace Paradox.Common
{

    public sealed class ModEntry : ReactiveObject
    {
        private ModDefinitionFile _modDefinitionFile;
        private ModsRegistryEntry _modsRegistryEntry;
        private bool _isChecked;
        private int _originalSpot;
        private bool _isPointerOver;
        private readonly ModManager _modManager;
        private SupportedVersion _supportedVersion;
        private bool _outdated;
        private ObservableCollection<LoadOrderConflict> _modConflicts;
        private ObservableHashSet<string> _idDependencies;
        private ObservableHashSet<string> _nameDependencies;
        private bool _overwritesOthers;
        private bool _overwrittenByOthers;
        private bool _allFilesOverwritten;

        public ModEntry(ModManager modManager)
        {
            this._modManager = modManager;
        }

        public ModDefinitionFile ModDefinitionFile
        {
            get => this._modDefinitionFile;
            set => this.RaiseAndSetIfChanged(ref this._modDefinitionFile, value);
        }

        public ModsRegistryEntry ModsRegistryEntry
        {
            get => this._modsRegistryEntry;
            set => this.RaiseAndSetIfChanged(ref this._modsRegistryEntry, value);
        }

        public ObservableCollection<LoadOrderConflict> ModConflicts
        {
            get => this._modConflicts;
            set => this.RaiseAndSetIfChanged(ref this._modConflicts, value);
        }

        public string DisplayName => this.ModDefinitionFile?.Name;

        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                if (this._isChecked != value && this._modManager.Loaded)
                {
                    if (value)
                    {
                        this._modManager.Enabled.Add(this);
                    }
                    else
                    {
                        this._modManager.Enabled.Remove(this);
                    }
                    this._modManager.Validate();
                }
                this.RaiseAndSetIfChanged(ref this._isChecked, value);
            }
        }

        public bool IsPointerOver
        {
            get => this._isPointerOver;
            set => this.RaiseAndSetIfChanged(ref this._isPointerOver, value);
        }

        public int OriginalSpot
        {
            get => this._originalSpot;
            set => this.RaiseAndSetIfChanged(ref this._originalSpot, value);
        }

        public bool Outdated
        {
            get => this._outdated;
            set => this.RaiseAndSetIfChanged(ref this._outdated, value);
        }

        public ObservableHashSet<string> IdDependencies
        {
            get => this._idDependencies;
            set => this.RaiseAndSetIfChanged(ref this._idDependencies, value);
        }

        public ObservableHashSet<string> NameDependencies
        {
            get => this._nameDependencies;
            set => this.RaiseAndSetIfChanged(ref this._nameDependencies, value);
        }

        public bool OverwritesOthers
        {
            get => this._overwritesOthers;
            set => this.RaiseAndSetIfChanged(ref this._overwritesOthers , value);
        }

        public bool OverwrittenByOthers
        {
            get => this._overwrittenByOthers;
            set => this.RaiseAndSetIfChanged(ref this._overwrittenByOthers , value);
        }

        public bool AllFilesOverwritten
        {
            get => this._allFilesOverwritten;
            set => this.RaiseAndSetIfChanged(ref this._allFilesOverwritten , value);
        }

 
        public void FillSupportedVersion()
        {
            if (!string.IsNullOrWhiteSpace(this.ModDefinitionFile.SupportedVersion) && Regex.IsMatch(this.ModDefinitionFile.SupportedVersion,
                @"((\d+)|\*)\.((\d+)|\*)\.((\d+)|\*)"))
            {
                this._supportedVersion = new SupportedVersion(this.ModDefinitionFile.SupportedVersion);
                this.Outdated = this._supportedVersion < this._modManager.Version;
            }
            else if (!string.IsNullOrWhiteSpace(this.ModDefinitionFile.Version) && Regex.IsMatch(this.ModDefinitionFile.Version,
                @"((\d+)|\*)\.((\d+)|\*)\.((\d+)|\*)"))
            {
                this._supportedVersion = new SupportedVersion(this.ModDefinitionFile.Version);
                this.Outdated = this._supportedVersion < this._modManager.Version;
            }
            else
            {
                this.Log().Error($"{this._modDefinitionFile.ModDefinitionFilePath} - Invalid supported_version");
                this._supportedVersion = new SupportedVersion(0, 0, 0);
                this.Outdated = true;
            }
        }
    }
}