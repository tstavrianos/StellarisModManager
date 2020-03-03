using System.Collections.Generic;
using System.Threading.Tasks;
using StellarisModManager.PDXModLib.ModData;

namespace StellarisModManager.PDXModLib.Interfaces
{
    public interface IInstalledModManager
    {
        IEnumerable<Mod> Mods { get; }

        void Initialize();
        void LoadMods();
        Task<bool> SaveMergedModAsync(MergedMod mod, bool mergeResultsOnly);
    }
}