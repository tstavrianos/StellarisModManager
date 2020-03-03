using System;
using System.Collections.Generic;
using StellarisModManager.PDXModLib.ModData;

namespace StellarisModManager.PDXModLib.Interfaces
{
    public interface IModConflictCalculator
    {
        ModConflictDescriptor CalculateConflicts(Mod mod);

        bool HasConflicts(ModFile file, Func<Mod, bool> modFilter);
        IEnumerable<ModConflictDescriptor> CalculateAllConflicts();
    }
}