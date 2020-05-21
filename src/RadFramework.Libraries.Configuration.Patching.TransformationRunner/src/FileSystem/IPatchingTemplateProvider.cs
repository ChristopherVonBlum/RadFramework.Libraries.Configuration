using RadFramework.Libraries.Configuration.Patching.Models;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner.FileSystem
{
    public interface IPatchingTemplateProvider
    {
        PatchingTemplate GetTemplate(ITransformationContext transformationContext, string templateName);
    }
}