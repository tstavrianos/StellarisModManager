namespace PDXModLib.Interfaces
{
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task<bool> RequestConfirmationAsync(string message, string title);

        Task ShowMessageAsync(string message, string title);
    }
}