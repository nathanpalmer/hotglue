using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public static class ScriptHelper
    {
        public static string Reference(
            HelperContext context,
            HelperOptions options,
            string root,
            IEnumerable<string> names)
        {
            var package = Package.Build(context.Configuration, root);
            var references = new List<SystemReference>();

            if (context.Debug)
            {
                foreach (var name in names)
                {
                    var cleanedName = name.Reslash();

                    string file = cleanedName.StartsWith("/")
                                      ? cleanedName.Substring(1)
                                      : Path.Combine(context.Configuration.ScriptPath.Reslash(), cleanedName).Reslash();
                    file = file.StartsWith("/") ? file.Substring(1) : file;

                    cleanedName = file.Substring(file.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    var reference = new SystemReference(new DirectoryInfo(root), new FileInfo(Path.Combine(root, file)), cleanedName);

                    references.AddRange(context.Locator.Load(root, reference));
                }
                
                return package.GenerateReferences(references, options);
            }

            foreach (var name in names)
            {
                var appName = name + "-glue";
                var appDirectory = new DirectoryInfo(root);
                var appFile = new FileInfo(Path.Combine(root + context.Configuration.ScriptPath, appName));
                var appReference = new SystemReference(appDirectory, appFile, appName) { Type = Model.Reference.TypeEnum.App };
                references.Add(appReference);                
            }

            return package.GenerateReferences(references, options);
        }

        public static void RewriteContent(
            LoadedConfiguration configuration,
            IReferenceLocator locator,
            string root,
            string fullPath,
            Func<String,String> queryString,
            Action<String,String> returnPhysicalFile,
            Action<String,String> returnTransformedContent
            )
        {
            var contentType = "application/x-javascript";

            var extension = Path.GetExtension(fullPath);
            var compiledExtension = configuration.Compilers.Any(x => x.Extensions.Contains(extension));
            if (!compiledExtension && File.Exists(fullPath))
            {
                returnPhysicalFile(fullPath, contentType);
                return;
            }

            // find references
            Reference.TypeEnum type = fullPath.EndsWith("-module")
                                          ? Model.Reference.TypeEnum.Module
                                          : fullPath.EndsWith("-glue")
                                                ? Model.Reference.TypeEnum.App
                                                : fullPath.EndsWith("-gen") 
                                                ? Model.Reference.TypeEnum.Generated
                                                : Model.Reference.TypeEnum.Dependency;

            var package = Package.Build(configuration, root);
            string content = null;

            if (fullPath.EndsWith("js-require"))
            {
                content = package.CompileStitch();
            }
            else
            {
                var name = queryString("name");
                var reference = SystemReference.Build(type, fullPath, root, name ?? "");

                switch (type)
                {
                    case Model.Reference.TypeEnum.App:
                        var references = locator.Load(root, reference);
                        content = package.Compile(references);
                        break;
                    case Model.Reference.TypeEnum.Dependency:
                        content = package.CompileDependency(reference);
                        break;
                    case Model.Reference.TypeEnum.Module:
                    case Model.Reference.TypeEnum.Generated:
                        content = package.CompileModule(reference);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            returnTransformedContent(content, contentType);
        }
    }
}
