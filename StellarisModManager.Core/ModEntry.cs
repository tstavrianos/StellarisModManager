using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Stellaris.Data.Json;
using Stellaris.Data;

namespace StellarisModManager.Core
{
    public sealed class ModEntry : INotifyPropertyChanged
    {
        private static int _entries;

        private bool _isEnabled;

        private bool _loaded;

        public bool Loaded
        {
            get => this._loaded;
            set
            {
                this._loaded = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => this._isEnabled;
            set
            {
                this._isEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public Guid Guid { get; }

        public Mod ModData { get; }

        public ModsRegistryEntry RegistryData { get; }

        public int OriginalSpot { get; }

        public string Name => this.ModData.Name;

        public IList<string> Issues { get; }

        public string IssuesHtml => string.Join('\n', this.Issues);

        public bool Overwrites { get; set; }
        public bool IsOverwritten { get; set; }

        public ModEntry(Mod mod, string guid, ModsRegistryEntry entry)
        {
            this.OriginalSpot = _entries++;
            this.Guid = guid != null ? Guid.Parse(guid) : Guid.NewGuid();
            this.RegistryData = entry;
            this.ModData = mod;
            this.Issues = new List<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return this.ModData.Name;
        }
    }
}