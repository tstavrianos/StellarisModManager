using System.ComponentModel;

namespace StellarisModManager.Core.Interfaces
{
    public interface IPropertyChangedModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        void OnPropertyChanged(string methodName);

        /// <summary>
        /// Called when [property changing].
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        void OnPropertyChanging(string methodName);
    }
}