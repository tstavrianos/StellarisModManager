using System.Text.RegularExpressions;
using Paradox.Common.Json;
using ReactiveUI;
using Splat;

namespace Paradox.Common
{
    using System.Collections.ObjectModel;

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

        private ObservableCollection<ModConflict> _modConflicts;

        public ModEntry(ModManager modManager)
        {
            this._modManager = modManager;
        }

        public ModDefinitionFile ModDefinitionFile
        {
            get => this._modDefinitionFile;
            set
            {
                this.RaiseAndSetIfChanged(ref this._modDefinitionFile, value);
                if (this._modDefinitionFile == null) return;
                if (this._supportedVersion != null)
                    this.Outdated = this._supportedVersion < this._modManager.Version;
                else if (!string.IsNullOrWhiteSpace(value.SupportedVersion) && Regex.IsMatch(value.SupportedVersion,
                    @"((\d+)|\*)\.((\d+)|\*)\.((\d+)|\*)"))
                {
                    this._supportedVersion = new SupportedVersion(value.SupportedVersion);
                    this.Outdated = this._supportedVersion < this._modManager.Version;
                }
                else if (!string.IsNullOrWhiteSpace(value.Version) && Regex.IsMatch(value.Version,
                    @"((\d+)|\*)\.((\d+)|\*)\.((\d+)|\*)"))
                {
                    this._supportedVersion = new SupportedVersion(value.Version);
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

        public ModsRegistryEntry ModsRegistryEntry
        {
            get => this._modsRegistryEntry;
            set => this.RaiseAndSetIfChanged(ref this._modsRegistryEntry, value);
        }

        public ObservableCollection<ModConflict> ModConflicts
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
    }
}