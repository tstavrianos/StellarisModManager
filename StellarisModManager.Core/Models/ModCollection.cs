using System.Collections.Generic;
using StellarisModManager.Core.Interfaces;

namespace StellarisModManager.Core.Models
{
    public class ModCollection : BaseModel, IModCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModCollection" /> class.
        /// </summary>
        public ModCollection()
        {
            this.Mods = new List<string>();
        }

        /// <summary>
        /// Gets or sets the game.
        /// </summary>
        /// <value>The game.</value>
        public string Game { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the mods.
        /// </summary>
        /// <value>The mods.</value>
        public IEnumerable<string> Mods { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
   }
}