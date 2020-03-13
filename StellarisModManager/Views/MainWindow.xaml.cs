using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using StellarisModManager.ViewModels;

namespace StellarisModManager.Views
{
    public sealed class MainWindow : Window
    {
        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel(this);
        }

        public void StartMoveOperation(object sender, PointerPressedEventArgs e) => this.ViewModel.StartMoveOperation(sender, e);

        public void MoveOperation(object sender, PointerEventArgs e) => this.ViewModel.MoveOperation(sender, e);

        public void EndMoveOperation(object sender, PointerReleasedEventArgs e) => this.ViewModel.EndMoveOperation(sender, e);

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}