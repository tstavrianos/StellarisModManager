using System;

namespace Paradox.Common.Parsers.pck
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class TransformAttribute : Attribute
    {
        public TransformAttribute(string name, string fromExtension, string toExtension, string description) : this(name)
        {
            this.FromExtension = fromExtension;
            this.ToExtension = toExtension;
            this.Description = description;
        }
        public TransformAttribute(string name)
        {
            if (null == name)
                throw new ArgumentNullException(nameof(name));
            if ("" == name)
                throw new ArgumentException("The name cannot be empty.", nameof(name));
            this.Name = name;
        }

        private string Name { get; set; } = null;
        private string FromExtension { get; set; } = null;
        private string ToExtension { get; set; } = null;
        private string Description { get; set; } = null;
    }
}
