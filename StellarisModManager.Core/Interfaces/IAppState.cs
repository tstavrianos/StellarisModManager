namespace StellarisModManager.Core.Interfaces
{
    public interface IAppState : IModel
    {
        /// <summary>
        /// Gets or sets the collection mods search term.
        /// </summary>
        /// <value>The collection mods search term.</value>
        string CollectionModsSearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the collection mods selected mod.
        /// </summary>
        /// <value>The collection mods selected mod.</value>
        string CollectionModsSelectedMod { get; set; }

        /// <summary>
        /// Gets or sets the collection mods sort column.
        /// </summary>
        /// <value>The collection mods sort column.</value>
        string CollectionModsSortColumn { get; set; }

        /// <summary>
        /// Gets or sets the collection mods sort mode.
        /// </summary>
        /// <value>The collection mods sort mode.</value>
        int CollectionModsSortMode { get; set; }

        /// <summary>
        /// Gets or sets the installed mods search term.
        /// </summary>
        /// <value>The installed mods search term.</value>
        string InstalledModsSearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the installed mods sort column.
        /// </summary>
        /// <value>The installed mods sort column.</value>
        string InstalledModsSortColumn { get; set; }

        /// <summary>
        /// Gets or sets the installed mods sort mode.
        /// </summary>
        /// <value>The installed mods sort mode.</value>
        int InstalledModsSortMode { get; set; }
    }

}