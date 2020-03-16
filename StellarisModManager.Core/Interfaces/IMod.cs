using StellarisModManager.Core.Enums;

namespace StellarisModManager.Core.Interfaces
{
    public interface IMod : IModObject
    {
        /// <summary>
        /// Gets or sets the descriptor file.
        /// </summary>
        /// <value>The descriptor file.</value>
        string DescriptorFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>The order.</value>
        int Order { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        ModSource Source { get; set; }
    }
}