using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Stellaris.Data;
using Stellaris.Data.Json;

namespace StellarisModManager.Core
{
    public sealed class ModEntry: INotifyPropertyChanged
    {
        private static int _entries;

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => this._isEnabled;
            set
            {
                this._isEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                this._isSelected = value;
                this.RaisePropertyChanged();
            }
        }

        public Guid Guid { get; }

        public Mod ModData { get; }

        public ModsRegistryEntry RegistryData { get; }

        public int OriginalSpot { get; }
        
        public string Name => this.ModData.Name;
        
        public IList<string> Issues { get; }

        public string IssuesHtml => string.Join("<br/>", this.Issues);
        
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