using StellarisModManager.Core.Interfaces;

namespace StellarisModManager.Core.Models
{
    public class AppState : BaseModel, IAppState
    {
        /// <summary>
        /// Gets or sets the collection mods search term.
        /// </summary>
        /// <value>The collection mods search term.</value>
        public virtual string CollectionModsSearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the collection mods selected mod.
        /// </summary>
        /// <value>The collection mods selected mod.</value>
        public virtual string CollectionModsSelectedMod { get; set; }

        /// <summary>
        /// Gets or sets the collection mods sort column.
        /// </summary>
        /// <value>The collection mods sort column.</value>
        public virtual string CollectionModsSortColumn { get; set; }

        /// <summary>
        /// Gets or sets the collection mods sort mode.
        /// </summary>
        /// <value>The collection mods sort mode.</value>
        public virtual int CollectionModsSortMode { get; set; }

        /// <summary>
        /// Gets or sets the installed mods search term.
        /// </summary>
        /// <value>The installed mods search term.</value>
        public virtual string InstalledModsSearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the installed mods sort column.
        /// </summary>
        /// <value>The installed mods sort column.</value>
        public virtual string InstalledModsSortColumn { get; set; }

        /// <summary>
        /// Gets or sets the installed mods sort mode.
        /// </summary>
        /// <value>The installed mods sort mode.</value>
        public virtual int InstalledModsSortMode { get; set; }
   }
}