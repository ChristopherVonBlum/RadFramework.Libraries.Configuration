using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RadFramework.Libraries.Configuration.Patching.Models
{
    public class TransformationContext : ITransformationContext
    {
        public ConcurrentDictionary<string, object> Cache { get; set; }= new ConcurrentDictionary<string, object>();
        private IDictionary<Type, object> policies = new Dictionary<Type, object>();

        public void SetPluginPolicy<T>(T value)
        {
            policies[typeof(T)] = value;
        }

        public T GetOrAddCacheEntry<T>(string key, Func<T> createEntry)
        {
            return (T)Cache.GetOrAdd(key, (k) => createEntry());
        }

        public bool HasPluginPolicy<T>()
        {
            return policies.ContainsKey(typeof(T));
        }

        public T GetPluginPolicy<T>()
        {
            return (T)policies[typeof(T)];
        }
    }
}