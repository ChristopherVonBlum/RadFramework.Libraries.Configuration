using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro
{
    public class PolicyPatch : IPolicyPatch
    {
        public PluginIncludeConditionDelegate[] ConditionDelegates;

        public JObject Conditions { get; set; }
        
        public string PatchTargetQuery { get; set; }

        public string IncludeFile { get; set; }

        public JToken Patch { get; set; }

        public OverrideDeltaMode OverrideDeltaMode { get; set; } = OverrideDeltaMode.AssignProperties;

        public string PatchSourcePath { get; set; }
        public JObject Match { get; set; } = new JObject();

        public bool CanApply(ITransformationContext transformationContext)
        {
            bool canApply = true;

            if (Conditions == null)
            {
                return true;
            }

            foreach (PluginIncludeConditionDelegate pluginIncludeConditionDelegate in ConditionDelegates)
            {
                canApply &= pluginIncludeConditionDelegate(transformationContext, Conditions);
            }

            return canApply;
        }

        public IEnumerable<JObject> GetPolicies()
        {
            if (Patch.Type == JTokenType.Array)
            {
                return Patch.Children<JObject>();
            }
            else if(Patch.Type == JTokenType.Object)
            {
                return new[] { (JObject)Patch };
            }

            return new List<JObject>();
        }
    }
}
