using System.Collections.Generic;
using System.Threading.Tasks;
using StellarisModManager.PDXModLib.GameContext;
using StellarisModManager.PDXModLib.ModData;

namespace StellarisModManager.PDXModLib.Interfaces
{
    public interface IGameContext
    {
        ModSelection CurrentSelection { get; set; }
        IEnumerable<ModSelection> Selections { get; }

        Task<bool> InitializeAsync();

        bool SaveSettings();
        bool SaveSelection();
        Task<bool> SaveMergedModAsync(MergedMod mod, bool mergedFilesOnly);
        void DeleteCurrentSelection();
        void DuplicateCurrentSelection(string newName);
        void LoadMods();
    }
}