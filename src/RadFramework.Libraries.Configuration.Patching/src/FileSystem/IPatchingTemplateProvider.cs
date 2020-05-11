using RadFramework.Libraries.Configuration.Patching.Plugins;

namespace RadFramework.Libraries.Configuration.Patching.FileSystem
{
    public interface IPatchingTemplateProvider
    {
        PatchingTemplate GetTemplate(ITransformationContext transformationContext, string templateName);
    }
}