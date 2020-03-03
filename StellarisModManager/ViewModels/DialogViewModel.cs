using System;
using ReactiveUI;

namespace StellarisModManager.ViewModels
{
    public class DialogViewModel<TResult> : WindowViewModel
    {
        private TResult _result;
        public TResult Result
        {
            get => this._result; 
            set => this.RaiseAndSetIfChanged(ref this._result, value);
        }

        public event EventHandler Closing; 

        protected DialogViewModel()
        {
        }

        protected void OnClosing()
        {
            this.Closing?.Invoke(this, EventArgs.Empty);
        }
    }

}