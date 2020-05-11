using RadFramework.Libraries.Configuration.Patching.Arguments;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner.Configuration
{
    public class InitConfig : IInitConfig
    {
        public bool Debug { get; set; }
        public IncludeContext IncludeContext { get; set; }
    }
}
