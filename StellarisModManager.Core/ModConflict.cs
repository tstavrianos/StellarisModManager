using System.Collections.Generic;

namespace StellarisModManager.Core
{
    public sealed class ModConflict
    {
        private readonly ModEntry _mod1;
        private readonly ModEntry _mod2;
        private readonly List<string> _conflictFiles;
        
        public ModConflict(ModEntry mod1, ModEntry mod2)
        {
            this._mod1 = mod1;
            this._mod2 = mod2;
            this._conflictFiles = new List<string>();
        }

        public void AddConflictFile(string file)
        {
            this._conflictFiles.Add(file);
        }

        public IReadOnlyList<string> ConflictFiles => this._conflictFiles;
    }
}