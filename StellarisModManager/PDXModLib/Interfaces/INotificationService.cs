using System.Threading.Tasks;

namespace StellarisModManager.PDXModLib.Interfaces
{
    public interface INotificationService
    {
        Task<bool> RequestConfirmationAsync(string message, string title);

        Task ShowMessageAsync(string message, string title);
    }
}