namespace PDXModLib.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PDXModLib.ModData;

    public interface IInstalledModManager
    {
        IEnumerable<Mod> Mods { get; }

        void Initialize();
        void LoadMods();
        Task<bool> SaveMergedModAsync(MergedMod mod, bool mergeResultsOnly);
    }
}