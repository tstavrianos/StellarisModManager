using System;
using PDXModLib.ModData;

namespace StellarisModManager
{
    using StellarisModManager.Models;

    public sealed class ItemPresenter : Presenter
    {
        private static int _ordering;
        public Mod Mod { get; }

        public ModsRegistryEntry Entry { get; set; }
        public Guid Guid { get; }
        public int OriginalSpot { get; }

        private bool _isSelected;

        public string Name => this.Mod.Name;

        private bool _isChecked;

        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                this._isChecked = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                this._isSelected = value;
                this.RaisePropertyChanged();
            }
        }

        public ItemPresenter(Mod mod)
        {
            this.Mod = mod;
            this.Guid = Guid.NewGuid();
            this.OriginalSpot = _ordering++;
        }

        public ItemPresenter(Mod mod, string guid)
        {
            this.Mod = mod;
            this.Guid = Guid.Parse(guid);
            this.OriginalSpot = _ordering++;
        }

        public override string ToString()
        {
            return this.Mod.Name;
        }
    }
}
