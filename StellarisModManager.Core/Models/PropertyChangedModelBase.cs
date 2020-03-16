using System.ComponentModel;
using StellarisModManager.Core.Interfaces;

namespace StellarisModManager.Core.Models
{
    public abstract class PropertyChangedModelBase : IPropertyChangedModel
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property value is changing.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        public void OnPropertyChanged(string methodName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(methodName));
        }

        /// <summary>
        /// Called when [property changing].
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        public void OnPropertyChanging(string methodName)
        {
            this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(methodName));
        }
    }
}