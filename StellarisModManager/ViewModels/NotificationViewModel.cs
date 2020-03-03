using System.Windows;
using System.Windows.Input;
using ReactiveUI;

namespace StellarisModManager.ViewModels
{
    public enum NotificationType
    {
        Question,
        Information,
        Warning,
        Error
    }

    public enum ButtonTypes
    {
        Ok,
        OkCancel,
        YesNo
    }

    public enum DialogResult
    {
        Cancel = 0,
        Ok = 1,
        No = 0,
        Yes = 1,
    }


    public class NotificationViewModel : DialogViewModel<DialogResult>
    {
        private readonly ButtonTypes buttons;
        private bool isFocused;

        public delegate NotificationViewModel Factory(string message, string title, ButtonTypes buttons, NotificationType type);

        public NotificationViewModel(string message, string title = "Notification", ButtonTypes buttons = ButtonTypes.Ok, NotificationType type = NotificationType.Information)
        {
            this.Message = message;
            this.Title = title;
            this.Type = type;
            this.buttons = buttons;

            this.Result = DialogResult.Cancel;

            this.Ok = ReactiveCommand.Create(this.DoOk);
            this.Cancel = ReactiveCommand.Create(this.DoCancel);
        }

        public bool IsFocused
        {
            get => this.isFocused;
            set => this.RaiseAndSetIfChanged(ref this.isFocused, value);
        }

        public ICommand Ok { get; set; }
        public ICommand Cancel { get; set; }

        public string Message { get; set; }
        public string Title { get; set; }
        public NotificationType Type { get; }

        public Visibility ShowOk => this.buttons == ButtonTypes.Ok || this.buttons == ButtonTypes.OkCancel? Visibility.Visible : Visibility.Collapsed;
        public Visibility ShowCancel => this.buttons == ButtonTypes.OkCancel? Visibility.Visible : Visibility.Collapsed;
        public Visibility ShowYes => this.buttons == ButtonTypes.YesNo? Visibility.Visible : Visibility.Collapsed;
        public Visibility ShowNo => this.buttons == ButtonTypes.YesNo? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsInfo => this.Type == NotificationType.Information ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsQuestion => this.Type == NotificationType.Question? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsWarning => this.Type == NotificationType.Warning? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsError => this.Type == NotificationType.Error? Visibility.Visible : Visibility.Collapsed;

        private void DoOk()
        {
            this.Result = DialogResult.Ok;
            this.OnClosing();
        }

        private void DoCancel()
        {
            this.Result = DialogResult.Cancel;
            this.OnClosing();
        }
    }
}