using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins.TargetingInformationMakro
{
    public class TargetingInformationMakroPlugin : ITransformationEnginePlugin
    {
        private PropertyInfo[] targetingInformationProperties =
            typeof(TargetingInformation).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        public bool TryHandle(ITransformationContext transformationContext, JToken makroToken, string[] makroArguments)
        {
            if (makroArguments.Length != 2 || makroArguments[0] != "TARGET")
            {
                return false;
            }

            string makroString = makroToken.Value<string>();

            makroString = makroString.Replace(
                $"{{${string.Join(' ', makroArguments)}}}", 
                (string)targetingInformationProperties
                    .Single(p => string.Equals(p.Name, makroArguments[1], StringComparison.CurrentCultureIgnoreCase))
                    .GetValue(transformationContext.GetPluginPolicy<TargetingInformation>()));

            makroToken.Replace(new JValue(makroString));

            return true;
        }
    }
}