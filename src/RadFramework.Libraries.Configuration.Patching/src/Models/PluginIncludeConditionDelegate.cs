using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Plugins;

namespace RadFramework.Libraries.Configuration.Patching.Models
{
    public delegate bool PluginIncludeConditionDelegate(ITransformationContext transformationContext, JObject jObject);
}