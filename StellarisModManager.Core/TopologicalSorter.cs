using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarisModManager.Core
{
    public class TopologicalSorter<T>
    {
        private class Relations
        {
            public int Dependencies = 0;
            public readonly HashSet<T> Dependents = new HashSet<T>();
        }
 
        private readonly Dictionary<T, Relations> _map = new Dictionary<T, Relations>();
 
        public void Add(T obj)
        {
            if (!this._map.ContainsKey(obj)) this._map.Add(obj, new Relations());
        }
 
        public void Add(T obj, T dependency)
        {
            if (dependency.Equals(obj)) return;
 
            if (!this._map.ContainsKey(dependency)) this._map.Add(dependency, new Relations());
 
            var dependents = this._map[dependency].Dependents;

            if (dependents.Contains(obj)) return;
            dependents.Add(obj);
 
            if (!this._map.ContainsKey(obj)) this._map.Add(obj, new Relations());
 
            ++this._map[obj].Dependencies;
        }
 
        public void Add(T obj, IEnumerable<T> dependencies)
        {
            foreach (var dependency in dependencies) this.Add(obj, dependency);
        }
 
        public void Add(T obj, params T[] dependencies)
        {
            this.Add(obj, dependencies as IEnumerable<T>);
        }
 
        public Tuple<IEnumerable<T>, IEnumerable<T>> Sort()
        {
            List<T> sorted = new List<T>(), cycled = new List<T>();
            var map = this._map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
 
            sorted.AddRange(map.Where(kvp => kvp.Value.Dependencies == 0).Select(kvp => kvp.Key));
 
            for (var idx = 0; idx < sorted.Count; ++idx) sorted.AddRange(map[sorted[idx]].Dependents.Where(k => --map[k].Dependencies == 0));
 
            cycled.AddRange(map.Where(kvp => kvp.Value.Dependencies != 0).Select(kvp => kvp.Key));
 
            return new Tuple<IEnumerable<T>, IEnumerable<T>>(sorted, cycled);
        }
 
        public void Clear()
        {
            this._map.Clear();
        }
    }
}