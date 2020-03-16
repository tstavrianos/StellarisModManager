using System.Collections.Generic;
using StellarisModManager.Core.Interfaces;

namespace StellarisModManager.Core.Models
{
    public class ModObject : IModObject
    {
        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        public IEnumerable<string> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>The picture.</value>
        public string Picture { get; set; }

        /// <summary>
        /// Gets or sets the remote identifier.
        /// </summary>
        /// <value>The remote identifier.</value>
        public int? RemoteId { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }
    }
}