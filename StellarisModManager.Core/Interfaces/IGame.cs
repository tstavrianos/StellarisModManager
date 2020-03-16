namespace StellarisModManager.Core.Interfaces
{
    public interface IGame : IModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the steam application identifier.
        /// </summary>
        /// <value>The steam application identifier.</value>
        int SteamAppId { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the user directory.
        /// </summary>
        /// <value>The user directory.</value>
        string UserDirectory { get; set; }

        /// <summary>
        /// Gets or sets the workshop directory.
        /// </summary>
        /// <value>The workshop directory.</value>
        string WorkshopDirectory { get; set; }
   }

}