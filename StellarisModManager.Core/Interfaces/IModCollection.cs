using System.Collections.Generic;

namespace StellarisModManager.Core.Interfaces
{
    public interface IModCollection : IModel
    {
        /// <summary>
        /// Gets or sets the game.
        /// </summary>
        /// <value>The game.</value>
        string Game { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the mods.
        /// </summary>
        /// <value>The mods.</value>
        IEnumerable<string> Mods { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }
    }
}