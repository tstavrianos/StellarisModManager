using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarisModManager.PDXModLib.ModData
{
    public class ModConflictDescriptor
    {
        public ModConflictDescriptor(Mod mod, IEnumerable<ModFileConflictDescriptor> fileConflicts)
        {
            this.Mod = mod;
            this.FileConflicts = fileConflicts.ToList();
            this.ConflictingMods = this.FileConflicts.SelectMany(fc => fc.ConflictingModFiles).Select(mf => mf.SourceMod).Distinct();
        }

        public Mod Mod { get; }

        public IEnumerable<ModFileConflictDescriptor> FileConflicts { get; }

        public IEnumerable<Mod> ConflictingMods { get; }

        public ModConflictDescriptor Filter(Func<Mod, bool> filterFunc)
        {
            return new ModConflictDescriptor(this.Mod, this.FileConflicts.Select(fc => fc.Filter(filterFunc)));
        }
    }
}