using System.Collections.Generic;

namespace Paradox.Common
{
    /// <summary>
    /// Criteria to search for a CWNode
    /// </summary>
    public class NodeSearchCriteria {
        /// <summary>
        /// The Node key that must match.   Checked case-insensitively.
        /// </summary>
        public string NodeKey { get; set; }
        
        /// <summary>
        /// A keyvalue pair that the node must contain.  This KVP can either be just the value (where any KVP that has that value satisfies), just the key (where any KVP that has that key satisfies) or both (KVP must match both key and value).  The comparison is case-insensitive.
        /// </summary>
        /// <remarks>
        /// These are checked against the raw values by default, this is controlled with <see cref="SearchForKvpAgainstRawValues"/> and <see cref="SearchForKvpAgainstSubstitutedValues"/>
        /// </remarks>
        public KeyValuePair<string, string>? KeyValue { get; set; }
        
        /// <summary>
        /// KVP searches should be against raw values.  This is <c>true</c> by default.
        /// </summary>
        public bool SearchForKvpAgainstRawValues { get; set; }
        
        /// <summary>
        /// KVP searches should be against substituted values.  This is <c>false</c> by default.
        /// </summary>
        public bool SearchForKvpAgainstSubstitutedValues { get; set; }
        
        /// <summary>
        /// A value that the node must contain.  Checked case-insensitively.
        /// </summary>
        public string Value { get; set; }

        public NodeSearchCriteria() {
            this.SearchForKvpAgainstRawValues = true;
            this.SearchForKvpAgainstSubstitutedValues = false;
        }
    }
}