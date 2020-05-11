using System.Collections.Generic;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner.Configuration
{
    public interface IEnvironementDescription
    {
        IEnumerable<StageDefinition> Stages { get; }
        IEnumerable<string> Roles { get; }
    }
}