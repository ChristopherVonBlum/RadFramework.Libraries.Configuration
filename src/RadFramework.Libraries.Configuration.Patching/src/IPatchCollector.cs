using System.Collections.Generic;
using RadFramework.Libraries.Configuration.Patching.Arguments;
using RadFramework.Libraries.Configuration.Patching.Models;
using RadFramework.Libraries.Configuration.Patching.Plugins;

namespace RadFramework.Libraries.Configuration.Patching
{
    public interface IPatchCollector
    {
        IEnumerable<IPolicyPatch> GetBasePatches(ITransformationContext transformationContext, IIncludeContext includeContext, string patchExtension);
        IEnumerable<IPolicyPatch> GetTargetedPatches(ITransformationContext transformationContext, IIncludeContext includeContext, string patchExtension);
    }
}