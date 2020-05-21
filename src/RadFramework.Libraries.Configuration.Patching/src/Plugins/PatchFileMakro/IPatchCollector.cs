using System.Collections.Generic;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro
{
    public interface IPatchCollector
    {
        IEnumerable<IPolicyPatch> GetBasePatches(ITransformationContext transformationContext, IIncludeContext includeContext, string patchExtension);
        IEnumerable<IPolicyPatch> GetTargetedPatches(ITransformationContext transformationContext, IIncludeContext includeContext, string patchExtension);
    }
}