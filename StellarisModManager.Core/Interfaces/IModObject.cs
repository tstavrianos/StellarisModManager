using System.Collections.Generic;

namespace StellarisModManager.Core.Interfaces
{
    public interface IModObject
    {
        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        IEnumerable<string> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>The picture.</value>
        string Picture { get; set; }

        /// <summary>
        /// Gets or sets the remote identifier.
        /// </summary>
        /// <value>The remote identifier.</value>
        int? RemoteId { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        string Version { get; set; }
   }
}