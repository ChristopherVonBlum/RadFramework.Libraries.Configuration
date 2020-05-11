using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;

namespace RadFramework.Abstractions.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private ConcurrentDictionary<Type, object> sections = new ConcurrentDictionary<Type, object>();
        
        public ConfigurationProvider(string configFile)
        {
            object[] rawSections = JsonConvert.DeserializeObject<object[]>(File.ReadAllText(configFile), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            foreach (var rawSection in rawSections)
            {
                sections[rawSection.GetType()] = rawSection;
            }
            
        }
        public TSection GetSection<TSection>()
        {
            return (TSection)sections[typeof(TSection)];
        }
    }
}