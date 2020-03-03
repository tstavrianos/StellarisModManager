using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CWTools.Process;
using Serilog;
using StellarisModManager.PDXModLib.Interfaces;
using StellarisModManager.PDXModLib.ModData;
using StellarisModManager.PDXModLib.Utilities;

namespace StellarisModManager.PDXModLib.GameContext
{
    public class InstalledModManager : IInstalledModManager
    {
        private readonly IGameConfiguration _gameConfiguration;
        private readonly INotificationService _notificationService;
		private readonly ILogger _logger;
		private readonly List<Mod> _mods = new List<Mod>();

        public IEnumerable<Mod> Mods => this._mods;

        public InstalledModManager(IGameConfiguration gameConfiguration, INotificationService notificationService, ILogger logger)
        {
            this._gameConfiguration = gameConfiguration;
            this._notificationService = notificationService;
            this._logger = logger.ForContext<InstalledModManager>();
		}

        public void Initialize()
        {
            this.LoadMods();
        }

        public void LoadMods()
        {
            foreach (var file in Directory.EnumerateFiles(this._gameConfiguration.ModsDir, "*.mod"))
            {
				var fileName = Path.GetFileName(file);

                if (this.Mods.Any(m => m.Id == fileName))
                {
                    this._logger.Debug($"Mod file skipped as it is already loaded: {fileName}");

					continue;
                }

                this._logger.Debug($"Loading mod file: {file}");
				Mod mod = null;
                try
                {
                    mod = Mod.Load(file, this._logger);
                    mod.LoadFiles(this._gameConfiguration.BasePath);
                }
                catch (Exception exception)
                {
                    this._logger.Error(exception, $"Error loading Mod {fileName}");
                }

                if (mod != null)
                {
                    this._mods.Add(mod);
                }
            }
        }

        public async Task<bool> SaveMergedModAsync(MergedMod mod, bool mergeResultOnly)
        {
            try
            {
                this._logger.Debug($"Saving mod: {mod.FileName} to {this._gameConfiguration.ModsDir}");

				var path = Path.Combine(this._gameConfiguration.ModsDir, mod.FileName);

                var descPath = Path.Combine(this._gameConfiguration.ModsDir, $"{mod.FileName}.mod");

                if (Directory.Exists(path))
                {
                    if (!await this._notificationService.RequestConfirmationAsync("Overwrite existing mod?", "Overwrite mod").ConfigureAwait(false))
                        return true;
                    foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                        File.Delete(file);

                    Directory.Delete(path, true);
                    File.Delete(descPath);
                }

                var node = new Node(mod.Name) {AllChildren = mod.ToDescriptor().ToList()};
                var visitor = new PrintingVisitor();
				visitor.Visit(node);

				var contents = visitor.Result;

                File.WriteAllText(descPath, contents);

				var files = mod.Files.AsEnumerable();
				if (mergeResultOnly)
					files = files.OfType<MergedModFile>();

                using (var saver = new DiskFileSaver(path))
                {
                    foreach (var modFile in files)
                    {
                        modFile.Save(saver);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Error saving mod!");
                return false;
            }

            return true;
        }
    }
}