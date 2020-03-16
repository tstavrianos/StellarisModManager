using System.Collections.Generic;
using StellarisModManager.Core.Enums;
using StellarisModManager.Core.Interfaces;

namespace StellarisModManager.Core.Models
{
    public class Mod : BaseModel, IMod
    {
        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        public virtual IEnumerable<string> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the descriptor file.
        /// </summary>
        /// <value>The descriptor file.</value>
        public virtual string DescriptorFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public virtual string FileName { get; set; }

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
        /// Gets or sets the order.
        /// </summary>
        /// <value>The order.</value>
        public virtual int Order { get; set; }

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>The picture.</value>
        public virtual string Picture { get; set; }

        /// <summary>
        /// Gets or sets the remote identifier.
        /// </summary>
        /// <value>The remote identifier.</value>
        public virtual int? RemoteId { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public virtual ModSource Source { get; set; }

        /// <summary>
        /// Gets or sets the supported version.
        /// </summary>
        /// <value>The supported version.</value>
        public virtual string SupportedVersion { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public virtual IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public virtual string Version { get; set; }
    }
}