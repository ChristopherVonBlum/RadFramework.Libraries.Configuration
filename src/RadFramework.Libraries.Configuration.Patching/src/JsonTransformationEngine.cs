using System.Linq;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Logging;
using RadFramework.Libraries.Configuration.Patching.Models;
using RadFramework.Libraries.Configuration.Patching.Plugins;

namespace RadFramework.Libraries.Configuration.Patching
{
    public class JsonTransformationEngine
    {
        private readonly ITransformationEnginePlugin[] _enginePlugins;

        public JsonTransformationEngine(ITransformationEnginePlugin[] enginePlugins)
        {
            _enginePlugins = enginePlugins;
        }

        public void EvaluateMakros(ITransformationContext transformationContext, JArray documentObject)
        {
            transformationContext.GetPluginPolicy<ILogMessageSink>()
                .EnterBlock(logger =>
            {
                transformationContext.SetPluginPolicy(logger);

                JToken makroToken = null;

                while ((makroToken = FindNextMakroToken(documentObject)) != null)
                {
                    string makroValue = makroToken.Value<string>();

                    int startIndex = makroValue.IndexOf("{$");

                    makroValue = makroValue.Substring(startIndex);

                    int endIndex = makroValue.IndexOf("}");

                    makroValue = makroValue.Substring(2, endIndex - 2);

                    string[] makroArguments = makroValue.Split(" ");

                    foreach (ITransformationEnginePlugin plugin in _enginePlugins)
                    {
                        if (!plugin.TryHandle(transformationContext, makroToken, makroArguments))
                        {
                            continue;
                        }

                        break;
                    }
                }
            });
        }
        
        private JToken FindNextMakroToken(JArray runtimeTemplate)
        {
            return runtimeTemplate
                .Descendants()
                .FirstOrDefault(d => d is JValue v
                                  && v.Value is string s
                                  && s.Contains("{$")
                                  && s.Substring(s.IndexOf("{$")).Contains("}"));
        }
    }
}