using System.Collections.Generic;
using StellarisModManager.PDXModLib.ModData;

namespace StellarisModManager.PDXModLib.GameContext
{
    public sealed class ModSelection
    {
        private static int _counter = 0;

        public int Idx { get; set; }

        public string Name { get; set; }

        public List<Mod> Contents { get; } = new List<Mod>();

        public ModSelection(string name)
        {
            this.Name = name;
            this.Idx = _counter++;
        }
    }
}