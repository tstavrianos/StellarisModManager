namespace StellarisModManager.PDXModLib.Interfaces
{
    public interface IGameConfiguration :IDefaultGameConfiguration
    {
        bool SettingsDirectoryValid { get; }

        bool GameDirectoryValid { get; }
    }
}