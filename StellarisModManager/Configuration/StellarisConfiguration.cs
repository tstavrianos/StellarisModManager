using System;
using System.Collections.Generic;
using System.IO;
using PDXModLib.Interfaces;

namespace StellarisModManager.Configuration
{
    public class StellarisConfiguration: IGameConfiguration
    {
        public int AppId => 281990;
        public string GameName => "Stellaris";
        public string BasePath { get; }
        public string ModsDir { get; }
        public string SettingsPath { get; }
        public string BackupPath { get; }
        public string SavedSelections { get; }
        public string GameInstallationDirectory { get; }
        public IEnumerable<string> WhiteListedFiles { get; } = new[] {"description.txt", "modinfo.lua", "descriptor.mod", "readme.txt", "changelog.txt"};
        
        public StellarisConfiguration()
        {
            BasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Paradox Interactive\\Stellaris";
            //this.BasePath = Path.Combine(Environment.CurrentDirectory, "test");
            this.ModsDir = $"{this.BasePath}\\mod";
            this.SettingsPath = $"{this.BasePath}\\settings.txt";
            this.BackupPath = $"{this.BasePath}\\settings.bak";
            this.SavedSelections = $"{this.BasePath}\\saved_selections.txt";
        }

        public bool SettingsDirectoryValid { get; } = true;
        public bool GameDirectoryValid { get; }= true;
    }
}