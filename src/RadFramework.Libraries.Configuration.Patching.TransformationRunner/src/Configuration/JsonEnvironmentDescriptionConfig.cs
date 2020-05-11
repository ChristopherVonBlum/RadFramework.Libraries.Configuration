using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner.Configuration
{
    public class JsonEnvironmentDescriptionConfig : IEnvironementDescription
    {
        private readonly string _outputRoot;
        private List<StageDefinition> stageDefinitions;

        public IEnumerable<StageDefinition> Stages => stageDefinitions;

        public IEnumerable<string> Roles => stageDefinitions.SelectMany(s => s.Servers).SelectMany(srv => srv.Roles).Distinct();

        public JsonEnvironmentDescriptionConfig(string environmentDescriptionConfig, string outputRoot)
        {
            _outputRoot = outputRoot;
            stageDefinitions = JsonConvert
                .DeserializeObject<StageDefinition[]>(File.ReadAllText(environmentDescriptionConfig))
                .ToList();

            stageDefinitions.ForEach(s =>
            {
                s.Servers.ToList().ForEach(srv =>
                {
                    srv.OutputPath = Path.Combine(_outputRoot, s.Stage, srv.Server + srv.Replication);
                });
            });
        }
    }
}