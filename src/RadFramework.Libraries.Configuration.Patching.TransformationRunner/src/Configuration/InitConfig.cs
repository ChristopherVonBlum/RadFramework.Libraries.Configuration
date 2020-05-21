using RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner.Configuration
{
    public class InitConfig : IInitConfig
    {
        public bool Debug { get; set; }
        public IncludeContext IncludeContext { get; set; }
    }
}
