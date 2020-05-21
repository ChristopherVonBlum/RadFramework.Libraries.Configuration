using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Logging;
using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro
{
    public class JsonPatchFileMakroPlugin : ITransformationEnginePlugin
    {
        private readonly IPatchCollector _patchCollector;

        public JsonPatchFileMakroPlugin(IPatchCollector patchCollector)
        {
            _patchCollector = patchCollector;
        }

        public bool TryHandle(ITransformationContext transformationContext, JToken makro, string[] makroArguments)
        {
            if (makroArguments.Length != 2 || makroArguments[0] != "PATCH")
            {
                return false;
            }

            JArray array = (JArray)makro.Parent;

            int index = array.IndexOf(makro);

            array.RemoveAt(index);

            string extensionToken = makroArguments[1];

            IIncludeContext includeContext = transformationContext.GetPluginPolicy<IIncludeContext>();

            ILogMessageSink logger = transformationContext.GetPluginPolicy<ILogMessageSink>();

            IEnumerable<(IPolicyPatch, IEnumerable<JObject>)> basePatches =
                _patchCollector
                    .GetBasePatches(transformationContext, includeContext, $"{extensionToken}.json")
                    .Select(p => (p, p.GetPolicies()));

            foreach ((IPolicyPatch, IEnumerable<JObject>) patch in basePatches)
            {
                foreach (JObject policy in patch.Item2)
                {
                    string sourceTerm = CreateSourceTerm(policy, patch.Item1.PatchSourcePath, "Base");
                    AppendPatchSource(policy, sourceTerm);
                    logger.Message(sourceTerm);

                    array.Insert(index, policy);
                    index++;
                }
            }

            IEnumerable<IPolicyPatch> targetedPatches =
                _patchCollector
                    .GetTargetedPatches(transformationContext, includeContext, $"{extensionToken}.json");

            foreach (IPolicyPatch policy in targetedPatches)
            {
                ApplyPatch(logger, policy, array);
            }

            return true;
        }

        private void ApplyPatch(ILogMessageSink logger, IPolicyPatch patch, JArray policySet)
        {
            List<JObject> patchPolicies = patch.GetPolicies().ToList();
            
            if (patch.OverrideDeltaMode == OverrideDeltaMode.Replace)
            {
                ApplyReplacePatch(logger, patch, patchPolicies, policySet);
            }
            else if (patch.OverrideDeltaMode == OverrideDeltaMode.AssignProperties)
            {
                ApplyAssignPatch(logger, patch, patchPolicies, policySet);
            }
        }

        private void ApplyAssignPatch(ILogMessageSink logger, IPolicyPatch policyPatch, List<JObject> patchPolicies, JArray policySet)
        {
            foreach (JObject patchPolicy in patchPolicies)
            {
                JObject patchTarget = FindPatchTarget(policyPatch, policySet);

                string sourceTerm = CreateSourceTerm(patchPolicy, policyPatch.PatchSourcePath, "Assign");

                if (patchTarget == null)
                {
                    logger.Warning(
                        $"Patch {sourceTerm} was not applied because a matching base patch was not found.");
                    continue;
                }

                if (policyPatch.PatchTargetQuery != null)
                {
                    patchTarget = (JObject)patchTarget.SelectToken(policyPatch.PatchTargetQuery);

                    if (patchTarget == null)
                    {
                        logger.Warning(
                            $"Patch {sourceTerm} was not applied because a matching base patch target {policyPatch.PatchTargetQuery} was not found.");
                        continue;
                    }
                }

                foreach (JProperty patchProperty in patchPolicy.Properties())
                {
                    if (patchProperty.Name.StartsWith("§"))
                    {
                        continue;
                    }

                    patchTarget[patchProperty.Name] = patchProperty.Value;
                }

                AppendPatchSource(patchTarget, sourceTerm);

                logger.Message($"{sourceTerm}\n\twhere {JsonConvert.SerializeObject(policyPatch.Conditions)}");
            }
        }

        private void ApplyReplacePatch(ILogMessageSink logger, IPolicyPatch policyPatch, List<JObject> patchPolicies, JArray policySet)
        {
            foreach (JObject patchPolicy in patchPolicies)
            {
                JObject originalObject = FindPatchTarget(policyPatch, policySet);

                string sourceTerm = CreateSourceTerm(patchPolicy, policyPatch.PatchSourcePath, "Replace");

                if (originalObject == null)
                {
                    logger.Warning(
                        $"Patch {sourceTerm} was not applied because a matching base patch was not found.");
                    continue;
                }

                int index = policySet.IndexOf(originalObject);

                policySet.RemoveAt(index);
                
                policySet.Insert(index, patchPolicy);

                logger.Message($"{sourceTerm}\n\twhere {JsonConvert.SerializeObject(policyPatch.Conditions)}");
            }
        }

        private void AppendPatchSource(JObject o, string sourceTerm)
        {
            if (o["§patchSource"]?.Type == JTokenType.Array)
            {
                JArray array = ((JArray)o["§patchSource"]);
                array.Add(sourceTerm);
                o.Remove("§patchSource");
                o.Last.AddAfterSelf(new JProperty("§patchSource", array));
                return;
            }

            o["§patchSource"] = new JArray(sourceTerm);
        }

        private static string CreateSourceTerm(JObject o, string fullPath, string type)
        {
            string objectSpecifier = string.IsNullOrEmpty(o.Path) ? string.Empty : $".{o.Path}";
            return $"{type}:{fullPath}{objectSpecifier}";
        }

        private static JObject FindPatchTarget(IPolicyPatch policyPatch, JArray policySet)
        {
            return policySet
                .Children<JObject>()
                .SingleOrDefault(policy => policyPatch
                    .Match
                    .Properties()
                    .Where(p => !p.Name.StartsWith("§"))
                    .All(prop => JToken.DeepEquals(policy.Property(prop.Name), prop)));
        }
    }
}