using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins
{
    public interface ITransformationEnginePlugin
    {
        bool TryHandle(ITransformationContext transformationContext, JToken makro, string[] makroArguments);
    }
}