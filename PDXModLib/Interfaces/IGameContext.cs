namespace PDXModLib.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PDXModLib.GameContext;
    using PDXModLib.ModData;

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