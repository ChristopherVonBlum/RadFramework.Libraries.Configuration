using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RadFramework.Libraries.Configuration.Patching.Arguments;
using RadFramework.Libraries.Configuration.Patching.FileSystem;
using RadFramework.Libraries.Configuration.Patching.Logging;
using RadFramework.Libraries.Configuration.Patching.Models;
using RadFramework.Libraries.Configuration.Patching.Plugins;
using RadFramework.Libraries.Configuration.Patching.TransformationRunner.Configuration;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner
{
    class ConfigPatcher
    {
        private readonly IInitConfig _initConfig;
        private readonly IEnvironementDescription _environmentDescription;
        private readonly IPatchingTemplateProvider _templateProvider;
        private readonly JsonTransformationEngine _transformationEngine;
        private readonly ILogMessageSink _logMessageSink;

        public ConfigPatcher(
            IInitConfig initConfig,
            IEnvironementDescription environmentDescription,
            IPatchingTemplateProvider templateProvider,
            JsonTransformationEngine transformationEngine,
            ILogMessageSink logMessageSink)
        {
            _initConfig = initConfig;
            _environmentDescription = environmentDescription;
            _templateProvider = templateProvider;
            _transformationEngine = transformationEngine;
            _logMessageSink = logMessageSink;
        }

        public bool Build(IIncludeContext includeContext)
        {
            CleanPreviousOutput(includeContext);

            _logMessageSink.Message($"{nameof(includeContext.OutputRoot)}:{Path.GetFullPath(includeContext.OutputRoot)}");
            _logMessageSink.Message($"{nameof(includeContext.ConfigRoot)}:{Path.GetFullPath(includeContext.ConfigRoot)}");
            _logMessageSink.Message($"{nameof(includeContext.IncludeRoot)}:{Path.GetFullPath(includeContext.IncludeRoot)}");
            _logMessageSink.Message($"Resolved include paths: \n{string.Join("\n", includeContext.ResolvedIncludeRoots)}");

            Stopwatch sw = new Stopwatch();

            _logMessageSink.Message("Beginning generation process...");

            sw.Start();

            List<Task> tasks = new List<Task>();

            // The TransformationContext has caching capabilities that are based on a concurrent dictionary.
            // Create it here to share things like resolved files while applying transformations to different output files
            ConcurrentDictionary<string, object> objectCache = new ConcurrentDictionary<string, object>();

            foreach (string role in _environmentDescription.Roles)
            {
                foreach (StageDefinition stage in _environmentDescription.Stages)
                {
                    foreach (ServerDefinition server in stage.Servers)
                    {
                        if (!server.Roles.Contains(role))
                        {
                            continue;
                        }

                        if (!Directory.Exists(server.OutputPath))
                        {
                            Directory.CreateDirectory(server.OutputPath);
                        }

                        if (_initConfig.Debug)
                        {
                            PerformTransformations(objectCache, includeContext, server, role, stage);
                            continue;
                        }

                        tasks.Add(Task.Run(() =>
                        {
                            PerformTransformations(objectCache, includeContext, server, role, stage);
                        }));
                    }
                }
            }

            if (!_initConfig.Debug)
            {
                Task.WaitAll(tasks.ToArray());
            }

            sw.Stop();

            return DetermineResult(sw);
        }

        private static void CleanPreviousOutput(IIncludeContext includeContext)
        {
            if (Directory.Exists(includeContext.OutputRoot))
            {
                Directory.Delete(includeContext.OutputRoot, true);
                Directory.CreateDirectory(includeContext.OutputRoot);
            }
        }

        private bool DetermineResult(Stopwatch sw)
        {
            if (!_logMessageSink.HasErrors && !_logMessageSink.HasWarnings)
            {
                _logMessageSink.Message($"Generation was done successfully({sw.Elapsed.Seconds}s)...");
                return true;
            }

            if (_logMessageSink.HasErrors)
            {
                _logMessageSink.Error($"Generation was finished with errors({sw.Elapsed.Seconds}s)...");
                return false;
            }

            if (_logMessageSink.HasWarnings)
            {
                _logMessageSink.Warning($"Generation was finished with warnings({sw.Elapsed.Seconds}s)...");
            }

            return true;
        }

        private void PerformTransformations(
            ConcurrentDictionary<string, object> objectCache,
            IIncludeContext includeContext,
            ServerDefinition server,
            string role,
            StageDefinition stage)
        {
            _logMessageSink.EnterBlock(sink =>
            {
                string environmentPath = Path.Combine(server.OutputPath, $"{role}.json");

                try
                {
                    sink.Message($"------------------------------------------------------------------------------------------------------\nStarting {environmentPath}");

                    TargetingInformation targetingInformation = new TargetingInformation
                    {
                        Replication = server.Replication,
                        Role = role,
                        Stage = stage.Stage,
                        Server = server.Server
                    };

                    // This needs to be created per file creation thread
                    TransformationContext transformationContext = new TransformationContext();

                    // use the shared cache created in the wrapping service method to share resource reads.
                    transformationContext.Cache = objectCache;

                    PatchingTemplate template = _templateProvider.GetTemplate(transformationContext, targetingInformation.Role);
                    JArray runtimeTemplate = JArray.Parse(template.String);

                    transformationContext.SetPluginPolicy(includeContext);
                    transformationContext.SetPluginPolicy(targetingInformation);

                    transformationContext.SetPluginPolicy(sink);

                    _transformationEngine.EvaluateMakros(transformationContext, runtimeTemplate);

                    File.WriteAllText(environmentPath, JsonConvert.SerializeObject(runtimeTemplate, Formatting.Indented));

                    sink.Message($"Finished {environmentPath}\n------------------------------------------------------------------------------------------------------");
                }
                catch (Exception e)
                {
                    sink.Error($"Error while generating {environmentPath}...\n{e.ToString()}");
                }
            });
        }

        public static bool PluginIncludeConditionDelegate(ITransformationContext context, JObject conditionNode)
        {
            TargetingInformation targetingInformation = context.GetPluginPolicy<TargetingInformation>();

            JObject targetInformation = (JObject)conditionNode["targetInformation"];

            bool canApply = true;
            
            if (targetInformation["targetStages"] != null)
            {
                canApply &= targetInformation["targetStages"]
                    .Values<string>()
                    .Contains(targetingInformation.Stage);
            }

            if (targetInformation["targetServers"] != null)
            {
                canApply &= targetInformation["targetServers"]
                    .Values<string>()
                    .Contains(targetingInformation.Server);
            }

            if (targetInformation["targetRoles"] != null)
            {
                canApply &= targetInformation["targetRoles"]
                    .Values<string>()
                    .Contains(targetingInformation.Role);
            }

            if (targetInformation["targetReplications"] != null)
            {
                if (string.IsNullOrEmpty(targetingInformation.Replication))
                {
                    return false;
                }

                canApply &= targetInformation["targetReplications"]
                    .Values<string>()
                    .Contains(targetingInformation.Replication);
            }

            return canApply;
        }
    }
}
