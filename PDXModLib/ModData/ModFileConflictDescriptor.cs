namespace PDXModLib.ModData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ModFileConflictDescriptor
    {
        public ModFileConflictDescriptor(ModFile file, IEnumerable<ModFile> conflictingModFiles)
        {
            this.File = file;
            this.ConflictingModFiles = conflictingModFiles.ToList();
        }

        public ModFile File { get; }

        public IReadOnlyCollection<ModFile> ConflictingModFiles { get; }

        public override bool Equals(object obj)
        {
            return this.File?.Path?.Equals((obj as ModFileConflictDescriptor)?.File?.Path) ?? false;
        }

        public override int GetHashCode()
        {
            return this.File?.Path?.GetHashCode() ?? -1;
        }

        public ModFileConflictDescriptor Filter(Func<Mod, bool> filterFunc)
        {
            return new ModFileConflictDescriptor(this.File, this.ConflictingModFiles.Where(cmf => filterFunc(cmf.SourceMod)));
        }
    }
}