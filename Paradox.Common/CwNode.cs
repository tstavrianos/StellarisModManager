using System;
using System.Collections.Generic;
using System.Linq;
using Paradox.Common.Extensions;
using Paradox.Common.Interfaces;

namespace Paradox.Common
{
    /// <summary>
    /// A complex object in the paradox file that has many children.  Most top level items are Nodes.  
    /// </summary>
    public sealed class CwNode
    {
        public CwNode(string key) {
            this.Key = key;
        }
        
        /// <summary>
        /// The node key
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// The CWNode that is the parent of this CWNode - e.g. the CWNode that contains this CWNode. 
        /// </summary>
        /// <remarks>
        /// This will be <c>null</c> for the CWNode that represents a file.  
        /// </remarks>
        public CwNode Parent { get; private set; }

        /// <summary>
        /// All child nodes of this one.
        /// </summary>
        /// <remarks>
        /// Would really like to use a dictionary here, but duplicate node keys are entirely possible, as keys are often things like logical operators
        /// </remarks>
        public IList<CwNode> Nodes {
            get => this._nodes;
            set {
                this._nodes = value;
                this._nodes.ForEach(node => node.Parent = this);
            } 
        }

        /// <summary>
        /// All key value pairs with their raw values (e.g. no scripted variables substituted)
        /// </summary>
        /// <remarks>
        /// Would really like to use a dictionary here, but duplicate keys are entirely possible, as keys are often things like logical operators
        /// </remarks>
        public IList<CwKeyValue> RawKeyValues {
            get => this._rawKeyValues;
            set {
                this._rawKeyValues = value;
                this._rawKeyValues.ForEach(keyValue => keyValue.ParentNode = this);
            } 
        }

        private IList<ICwKeyValue> _keyValues;
        /// <summary>
        /// All key value pairs with their resolved values (e.g. scripted variables processed)
        /// </summary>
        /// <remarks>
        /// Would really like to use a dictionary here, but duplicate keys are entirely possible, as keys are often things like logical operators
        /// </remarks>
        public IEnumerable<ICwKeyValue> KeyValues {
            get { return this._keyValues ??= this.RawKeyValues.Select(x => (ICwKeyValue) new CwNodeContextedKeyValue(x, this.ScriptedVariablesAccessor)).ToList(); }
        }

        /// <summary>
        /// Straight values within the node, these are almost always comments.
        /// </summary>
        public IList<string> Values { get; set; }

        private IScriptedVariablesAccessor _scriptedVariablesAccessor;
        private IList<CwNode> _nodes;
        private IList<CwKeyValue> _rawKeyValues;

        public IScriptedVariablesAccessor ScriptedVariablesAccessor {
            get => this._scriptedVariablesAccessor ??= new DummyScriptedVariablesAccessor();
            set => this._scriptedVariablesAccessor = value;
        }

        /// <summary>
        /// Gets the first child node with the specified key.
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first node if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">They key</param>
        /// <returns></returns>
        public CwNode GetNode(string key)
        {
            return this.Nodes.FirstOrDefault(x => x.Key == key);
        }

        /// <summary>
        /// Get all child nodes with the specified key.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns></returns>
        public IEnumerable<CwNode> GetNodes(string key) {
            return this.Nodes.Where(x => x.Key == key);
        }
        
        /// <summary>
        /// If there are any child nodes with the given key, performs the specified Action on them.
        /// </summary>
        /// <param name="key">The key of the child nodes</param>
        /// <param name="perform">The Action to perform if any are found</param>
        public void ActOnNodes(string key, Action<CwNode> perform) {
            this.ActOnNodes(key, perform, () => { });
        }
        
        /// <summary>
        /// If there are any child nodes with the given key, performs the specified Action on them, otherwise perform the no match action.
        /// </summary>
        /// <param name="key">The key of the child nodes</param>
        /// <param name="perform">The Action to perform if any are found</param>
        /// <param name="performIfNoMatch">The Action to perform if there is no nodes with the specified key</param>
        public void ActOnNodes(string key, Action<CwNode> perform, Action performIfNoMatch) {
            var nodes = this.GetNodes(key);
            if (nodes.Any()) {
                foreach (var cwNode in nodes) {
                    perform(cwNode);
                }
            }
            else {
                performIfNoMatch();
            }
        }

        /// <summary>
        /// If there is a child key-value with the specified key, returns it.  Attempting to convert any variables using <see cref="ScriptedVariablesAccessor"/>
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">The Key of the Keyvalue item within the node</param>
        /// <returns>See above.</returns>
        public string GetKeyValue(string key) {
            var value = this.GetRawKeyValue(key);
            return this.ScriptedVariablesAccessor.GetPotentialValue(value);
        }
        
        /// <summary>
        /// If there is a child key-value with the specified key, returns it, otherwise uses the supplied default value.  Attempting to convert any variables using <see cref="ScriptedVariablesAccessor"/>
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">The Key of the Keyvalue item within the node</param>
        /// <param name="defaultValue">The value to use (and supplied to the <see cref="ScriptedVariablesAccessor"/>) if the keyvalue does not exist in the node</param>
        /// <returns>See above.</returns>
        public string GetKeyValueOrDefault(string key, object defaultValue) {
            var value = this.GetRawKeyValue(key);
            return this.ScriptedVariablesAccessor.GetPotentialValue(value ?? defaultValue.ToString());
        }
        
        /// <summary>
        /// If there are children key-value with the specified key, performs the specified action on them.  Attempting to convert any variables using <see cref="ScriptedVariablesAccessor"/>
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">The Key of the Keyvalue item within the node</param>
        /// <param name="perform">The action to perform if the value exists.</param>
        /// <returns>See above.</returns>
        public void ActOnKeyValues(string key, Action<string> perform) {
            this.KeyValues.Where(x => x.Key == key).Select(x => this.ScriptedVariablesAccessor.GetPotentialValue(x.Value)).ForEach(perform);
        }


        /// <summary>
        /// If there is a child key-value with the specified key, returns it. Does not perform variable conversion.
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">The Key of the Keyvalue item within the node</param>
        /// <returns>See above.</returns>
        public string GetRawKeyValue(string key) {
            return this.RawKeyValues.FirstOrDefault(x => x.Key == key)?.Value;
        }
    }
}