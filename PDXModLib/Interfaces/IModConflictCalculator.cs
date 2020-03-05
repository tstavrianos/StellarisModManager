namespace PDXModLib.Interfaces
{
    using System;
    using System.Collections.Generic;

    using PDXModLib.ModData;

    public interface IModConflictCalculator
    {
        ModConflictDescriptor CalculateConflicts(Mod mod);

        bool HasConflicts(ModFile file, Func<Mod, bool> modFilter);
        IEnumerable<ModConflictDescriptor> CalculateAllConflicts();
    }
}