using System.Threading.Tasks;

namespace PDXModLib.Utility
{
    public interface INotificationService
    {
        Task<bool> RequestConfirmation(string message, string title);

        Task ShowMessage(string message, string title);
    }
}
