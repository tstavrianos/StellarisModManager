using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Serilog;
using StellarisModManager.ViewModels;

namespace StellarisModManager.Views
{
    public sealed class MainWindow : Window
    {
        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        public MainWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.DataContext = new MainWindowViewModel(this, Log.Logger);
        }

        public void StartDrag(object sender, PointerPressedEventArgs e) => this.ViewModel.StartDrag(sender, e);

        public void DoDrag(object sender, PointerEventArgs e) => this.ViewModel.DoDrag(sender, e);

        public void EndDrag(object sender, PointerReleasedEventArgs e) => this.ViewModel.EndDrag(sender, e);

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}