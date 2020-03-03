using System;
using System.Collections.Generic;
using System.Linq;
using StellarisModManager.PDXModLib.Interfaces;
using StellarisModManager.PDXModLib.ModData;

namespace StellarisModManager.PDXModLib.GameContext
{
    public class ModConflictCalculator : IModConflictCalculator
    {
        private readonly IGameConfiguration _gameConfiguration;
        private readonly IInstalledModManager _installedModManager;

        public ModConflictCalculator(IGameConfiguration gameConfiguration, IInstalledModManager installedModManager)
        {
            this._gameConfiguration = gameConfiguration;
            this._installedModManager = installedModManager;
        }

        public ModConflictDescriptor CalculateConflicts(Mod mod)
        {
            var fileConflicts = mod.Files.Select(this.CalculateConflicts);
            return new ModConflictDescriptor(mod, fileConflicts);
        }

        public bool HasConflicts(ModFile file, Func<Mod, bool> modFilter)
        {
            return this._installedModManager.Mods.Where(m => !Equals(m, file.SourceMod) && modFilter(m)).SelectMany(m => m.Files).Any(mf => mf.Path == file.Path);
        }

        private bool ShouldCompare(ModFile mod)
        {
            return this._gameConfiguration.WhiteListedFiles.All(wlf => string.Compare(mod.Filename, wlf, StringComparison.OrdinalIgnoreCase) != 0);
        }

        private ModFileConflictDescriptor CalculateConflicts(ModFile modfile)
        {
            var conflictingModFiles = this.ShouldCompare(modfile)
                ? this._installedModManager.Mods.Where(m => !Equals(m, modfile.SourceMod))
                                           .Select(m => m.Files.FirstOrDefault(mf => mf.Path.Equals(modfile.Path)))
                                           .Where(mf => mf != null)
                : Enumerable.Empty<ModFile>();

            return new ModFileConflictDescriptor(modfile, conflictingModFiles);
        }

        public IEnumerable<ModConflictDescriptor> CalculateAllConflicts()
        {
            var source = this._installedModManager.Mods.ToDictionary(m => m, m => new List<List<ModFile>>());

            var allFiles = this._installedModManager.Mods.SelectMany(m => m.Files);

            var groupped = allFiles.GroupBy(m => m.Path);
            foreach (var conflictGroup in groupped)
            {
                var cgList = conflictGroup.ToList();
                foreach (var modFile in cgList)
                {
                    source[modFile.SourceMod].Add(cgList);
                }
            }

            foreach (var conflictSource in source)
            {
                var result = new List<ModFileConflictDescriptor>(100);
                var mod = conflictSource.Key;
                var files = conflictSource.Value;

                result.AddRange(from fileList in files
                    let file = fileList.First(f => Equals(f.SourceMod, mod))
                    select this.ShouldCompare(file)
                        ? new ModFileConflictDescriptor(file, fileList.Where(f => f != file))
                        : new ModFileConflictDescriptor(file, Enumerable.Empty<ModFile>()));

                yield return new ModConflictDescriptor(mod, result);
            }
        }
    }
}