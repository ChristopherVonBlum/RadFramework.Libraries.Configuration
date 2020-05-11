using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Plugins;

namespace RadFramework.Libraries.Configuration.Patching.Models
{
    public interface IPolicyPatch
    {
        JObject Conditions { get; }

        JObject Match { get; }

        OverrideDeltaMode OverrideDeltaMode { get; }
        
        string PatchTargetQuery { get; }

        string PatchSourcePath { get; set; }

        bool CanApply(ITransformationContext transformationContext);

        IEnumerable<JObject> GetPolicies();
    }

}