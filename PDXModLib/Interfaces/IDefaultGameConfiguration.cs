﻿using System.Collections.Generic;

namespace PDXModLib.Interfaces
{
    /// <summary>
    /// Interface describing the basic game configuration.
    /// </summary>
    public interface IDefaultGameConfiguration
    {
		/// <summary>
		/// Gets the steam app id for the game
		/// </summary>
		int AppId { get; }

		/// <summary>
		/// Gets the game name
		/// </summary>
		string GameName { get; }

		/// <summary>
		/// Gets the base game path
		/// </summary>
		string BasePath { get; }

        /// <summary>
        /// Gets the mods subfolder, usually a subFolder of BasePath
        /// </summary>
        string ModsDir { get; }

        /// <summary>
        /// Gets the path to the settings.txt file.
        /// This is the base game settings file.
        /// </summary>
        string SettingsPath { get; }

        /// <summary>
        /// Gets the path to the backup path of settings.txt
        /// </summary>
        string BackupPath { get; }

        /// <summary>
        /// Gets the path to the saved_selections.txt file.
        /// The file is used to save custom mod selections.
        /// </summary>
        string SavedSelections { get; }

		/// <summary>
		/// Gets the path to the game installtion directory.
		/// </summary>
		string GameInstallationDirectory { get; }

		/// <summary>
		/// Gets the list of file paths (relative to mod base path), that should not be considered for conflicts.
		/// </summary>
		IReadOnlyCollection<string>  WhiteListedFiles { get; }
    }
}
