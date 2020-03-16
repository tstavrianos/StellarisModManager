using StellarisModManager.Core.Interfaces;

namespace StellarisModManager.Core.Models
{
    public class Game : BaseModel, IGame
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the steam application identifier.
        /// </summary>
        /// <value>The steam application identifier.</value>
        public virtual int SteamAppId { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public virtual string Type { get; set; }

        /// <summary>
        /// Gets or sets the mod directory.
        /// </summary>
        /// <value>The mod directory.</value>
        public virtual string UserDirectory { get; set; }

        /// <summary>
        /// Gets or sets the workshop directory.
        /// </summary>
        /// <value>The workshop directory.</value>
        public virtual string WorkshopDirectory { get; set; }
   }
}