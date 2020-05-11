using Newtonsoft.Json.Linq;

namespace RadFramework.Libraries.Configuration.Patching.Plugins
{
    public interface ITransformationEnginePlugin
    {
        bool TryHandle(ITransformationContext transformationContext, JToken makro, string[] makroArguments);
    }
}