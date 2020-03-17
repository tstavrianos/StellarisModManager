using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradox.Common
{
    public static class CwNodeHelpers
    {
        public static bool? ResolveBooleanValue(string value) {
            if (value.Equals("yes", StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }
            
            if (value.Equals("no", StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            return null;
        }
        
        public static bool ResolveBoolean(bool startingValue, CwNode node) {
            bool endingValue;
            switch (node.Key.ToLowerInvariant()) {
                case "and":
                case "or":
                    endingValue = startingValue;
                    break;
                case "nor":
                case "not":
                    endingValue = !startingValue;
                    break;
                default: endingValue = startingValue;
                    break;
            }

            return node.Parent != null ? ResolveBoolean(endingValue, node.Parent) : endingValue;
        }

        public static CwNode SearchNodes(this IEnumerable<CwNode> nodes, NodeSearchCriteria nodeSearchCriteria)
        {
            return nodes.Select(cwNode => SearchNodes(cwNode, nodeSearchCriteria)).FirstOrDefault(searchNodeResult => searchNodeResult != null);
        }

        /// <summary>
        /// Finds a node in this CWNode and all of its child CWNodes that matches the specified <see cref="NodeSearchCriteria"/>.  It will return the first Node that matches, and what consitutes "first" is undetermined.
        /// </summary>
        /// <param name="nodeSearchCriteria">The criteria.  Null fields are ignored in searching.  If multiple criteria are specified then the match returns the first node that matches any of them</param>
        /// <returns>The found node, or <c>null</c> if none could be found</returns>
        public static CwNode SearchNodes(this CwNode node, NodeSearchCriteria nodeSearchCriteria) {
            // test if our node matches the criteria
            if (nodeSearchCriteria.NodeKey != null) {
                if (node.Key.Equals(nodeSearchCriteria.NodeKey, StringComparison.InvariantCultureIgnoreCase)) {
                    return node;
                }
            }

            if (nodeSearchCriteria.Value != null) {
                if (node.Values.Contains(nodeSearchCriteria.Value, StringComparer.InvariantCultureIgnoreCase)) {
                    return node;
                }
            }

            if (nodeSearchCriteria.KeyValue == null)
                return node.Nodes.Select(cwNode => cwNode.SearchNodes(nodeSearchCriteria))
                    .FirstOrDefault(childResult => childResult != null);
            var criteriaKeyValue = nodeSearchCriteria.KeyValue.Value;
            if (criteriaKeyValue.Key == null) {
                if (nodeSearchCriteria.SearchForKvpAgainstRawValues) {
                    if (node.RawKeyValues.Any(kvp => kvp.Value.Equals(criteriaKeyValue.Value, StringComparison.InvariantCultureIgnoreCase))) {
                        return node;
                    }
                }

                if (!nodeSearchCriteria.SearchForKvpAgainstSubstitutedValues)
                    return node.Nodes.Select(cwNode => cwNode.SearchNodes(nodeSearchCriteria))
                        .FirstOrDefault(childResult => childResult != null);
                {
                    if (node.KeyValues.Any(kvp => kvp.Value.Equals(criteriaKeyValue.Value, StringComparison.InvariantCultureIgnoreCase))) {
                        return node;
                    }
                }
            }
            else if (criteriaKeyValue.Value == null) {
                if (nodeSearchCriteria.SearchForKvpAgainstRawValues) {
                    if (node.RawKeyValues.Any(kvp => kvp.Key.Equals(criteriaKeyValue.Key, StringComparison.InvariantCultureIgnoreCase))) {
                        return node;
                    }
                }

                if (!nodeSearchCriteria.SearchForKvpAgainstSubstitutedValues)
                    return node.Nodes.Select(cwNode => cwNode.SearchNodes(nodeSearchCriteria))
                        .FirstOrDefault(childResult => childResult != null);
                {
                    if (node.KeyValues.Any(kvp => kvp.Key.Equals(criteriaKeyValue.Key, StringComparison.InvariantCultureIgnoreCase))) {
                        return node;
                    }
                }
            }
            else {
                if (nodeSearchCriteria.SearchForKvpAgainstRawValues) {
                    if (node.RawKeyValues.Any(kvp => kvp.Equals(criteriaKeyValue, StringComparison.InvariantCultureIgnoreCase))) {
                        return node;
                    }
                }

                if (!nodeSearchCriteria.SearchForKvpAgainstSubstitutedValues)
                    return node.Nodes.Select(cwNode => cwNode.SearchNodes(nodeSearchCriteria))
                        .FirstOrDefault(childResult => childResult != null);
                {
                    if (node.KeyValues.Cast<CwNodeContextedKeyValue>().Any(kvp => kvp.Equals(criteriaKeyValue, StringComparison.InvariantCultureIgnoreCase))) {
                        return node;
                    }
                }
            }

            // otherwise DFS down the children
            return node.Nodes.Select(cwNode => cwNode.SearchNodes(nodeSearchCriteria)).FirstOrDefault(childResult => childResult != null);
        }
    }
}