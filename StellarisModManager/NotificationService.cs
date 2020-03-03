using System.Threading.Tasks;
using Serilog;
using StellarisModManager.PDXModLib.Interfaces;
using StellarisModManager.Utilities;
using StellarisModManager.ViewModels;

namespace StellarisModManager
{
    internal class NotificationService : INotificationService
    {
        private ILogger _logger;
        private readonly IShowDialog<NotificationViewModel, DialogResult, string, string, ButtonTypes, NotificationType> _showWindow;

        public NotificationService(ILogger logger, IShowDialog<NotificationViewModel, DialogResult, string, string, ButtonTypes, NotificationType> showWindow )
        {
            this._logger = logger;
            this._showWindow = showWindow;
        }
        
        public async Task<bool> RequestConfirmationAsync(string message, string title)
        {
            return await this._showWindow.Show(message, title, ButtonTypes.YesNo, NotificationType.Question).ConfigureAwait(false) == DialogResult.Yes;
        }

        public Task ShowMessageAsync(string message, string title)
        {
            return this._showWindow.Show(message, title, ButtonTypes.Ok, NotificationType.Information);
        }
    }
}