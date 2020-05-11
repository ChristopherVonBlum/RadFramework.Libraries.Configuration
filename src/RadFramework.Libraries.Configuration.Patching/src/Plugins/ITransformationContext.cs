using System;

namespace RadFramework.Libraries.Configuration.Patching.Plugins
{
    public interface ITransformationContext
    {
        T GetOrAddCacheEntry<T>(string key, Func<T> createEntry);

        bool HasPluginPolicy<T>();
        T GetPluginPolicy<T>();
        void SetPluginPolicy<T>(T policy);
    }
}