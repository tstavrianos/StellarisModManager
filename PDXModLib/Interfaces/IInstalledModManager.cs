using System.Collections.Generic;
using System.Threading.Tasks;
using PDXModLib.ModData;

namespace PDXModLib.Interfaces
{
    public interface IInstalledModManager
    {
        IEnumerable<Mod> Mods { get; }

        void Initialize();
        void LoadMods();
        Task<bool> SaveMergedMod(MergedMod mod, bool mergeResultsOnly);
    }
}
