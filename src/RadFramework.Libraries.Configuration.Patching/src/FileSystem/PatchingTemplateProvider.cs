using System.IO;
using RadFramework.Libraries.Configuration.Patching.Plugins;

namespace RadFramework.Libraries.Configuration.Patching.FileSystem
{
    public class PatchingTemplateProvider : IPatchingTemplateProvider
    {
        private readonly string _templateFolder;
        private readonly string _templateExtension;

        public PatchingTemplateProvider(string templateFolder, string templateExtension)
        {
            _templateFolder = templateFolder;
            _templateExtension = templateExtension;
        }

        public PatchingTemplate GetTemplate(ITransformationContext transformationContext, string templateName)
        {
            string templatePath = Path.Combine(_templateFolder, templateName + _templateExtension);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(templatePath);
            }

            return transformationContext.GetOrAddCacheEntry(templatePath, () => new PatchingTemplate
            {
                Name = templateName,
                Path = templatePath,
                String = File.ReadAllText(templatePath)
            });
        }
    }
}