using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro
{
    public class PatchCollector : IPatchCollector
    {
        private readonly PluginIncludeConditionDelegate[] _conditionDelegates;

        public PatchCollector(PluginIncludeConditionDelegate[] conditionDelegates)
        {
            _conditionDelegates = conditionDelegates;
        }

        public IEnumerable<IPolicyPatch> GetBasePatches(
            ITransformationContext transformationContext,
            IIncludeContext includeContext,
            string patchExtension)
        {
            return CollectFromIncludeLocations(
                    transformationContext,
                    includeContext,
                    patchExtension)
                .Where(p => p.CanApply(transformationContext))
                .Where(p => !p.Match?.Properties()
                                .Any() ??
                            false)
                .ToArray();
        }

        public IEnumerable<IPolicyPatch> GetTargetedPatches(
            ITransformationContext transformationContext,
            IIncludeContext includeContext,
            string patchExtension)
        {
            return CollectFromIncludeLocations(
                    transformationContext,
                    includeContext,
                    patchExtension)
                        // patch applies by targeting information
                        .Where(p => p.CanApply(transformationContext))
                        // and has additional conditions (match property)
                        // this indicates that this is not a base patch
                        .Where(p => p.Match?.Properties().Any() ?? false);
        }

        private IEnumerable<IPolicyPatch> CollectFromIncludeLocations(
            ITransformationContext transformationContext,
            IIncludeContext includeContext,
            string patchToken)
        {
            List<PolicyPatch> policySet = new List<PolicyPatch>();

            ImmutableSortedDictionary<string, string> patches = GetPatchCache(transformationContext, includeContext, patchToken);

            foreach (KeyValuePair<string, string> file in patches)
            {
                LoadPatchFile(includeContext, file.Key, file.Value, policySet);
            }

            return policySet;
        }


        private ImmutableSortedDictionary<string, string> GetPatchCache(
            ITransformationContext transformationContext,
            IIncludeContext includeContext,
            string patchToken)
        {
            return transformationContext
                .GetOrAddCacheEntry(
                    includeContext.IncludeRoot + "|" + patchToken, 
                    () => includeContext
                            .ResolvedIncludeRoots
                            .SelectMany(r => Directory.GetFiles(r, $"*{patchToken}", SearchOption.AllDirectories))
                            .Distinct()
                            .Select(path=> (path, File.ReadAllText(path)))
                            .OrderBy(p => Path.GetRelativePath(includeContext.IncludeRoot, p.Item1))
                            .ToImmutableSortedDictionary(k => k.Item1, v => v.Item2));
        }

        private void LoadPatchFile(
            IIncludeContext includeContext, 
            string path, 
            string json,
            List<PolicyPatch> policySet)
        {
            JToken token = JToken.Parse(json);

            if (token is JArray patchArray)
            {
                patchArray
                    .Children<JObject>()
                    .ToList()
                    .ForEach(patch =>
                    {
                        LoadPatch(includeContext, path, policySet, patch);
                    });

                return;
            }

            if (token is JObject patchObj)
            {
                LoadPatch(includeContext, path, policySet, patchObj);
            }
        }

        private void LoadPatch(
            IIncludeContext includeContext, 
            string path, 
            List<PolicyPatch> policySet,
            JObject patch)
        {
            PolicyPatch p = patch.ToObject<PolicyPatch>();

            p.ConditionDelegates = _conditionDelegates;

            string patchPath = path;
            
            p.PatchSourcePath = Path.GetRelativePath(includeContext.IncludeRoot, path) + "=>" + patch.Path;

            // ref patches will be added in a nested call in the else statement
            if (p.IncludeFile != null)
            {
                string includePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(patchPath), p.IncludeFile));

                LoadPatchFile(includeContext, includePath, File.ReadAllText(includePath),  policySet);
            }
            else
            {
                policySet.Add(p);
            }
        }
    }
}