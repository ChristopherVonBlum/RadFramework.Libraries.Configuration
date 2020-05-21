using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro
{
    public delegate bool PluginIncludeConditionDelegate(ITransformationContext transformationContext, JObject jObject);
}