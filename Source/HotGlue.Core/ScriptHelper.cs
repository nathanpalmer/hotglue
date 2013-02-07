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
            LoadedConfiguration configuration, 
            IReferenceLocator locator,
            string root,
            string name,
            bool debug)
        {
            var package = Package.Build(configuration, root, null);

            if (debug)
            {
                name = name.Reslash();

                string file = name.StartsWith("/")
                                  ? name.Substring(1)
                                  : Path.Combine(configuration.ScriptPath.Reslash(), name).Reslash();
                file = file.StartsWith("/") ? file.Substring(1) : file;

                name = file.Substring(file.LastIndexOf("/", StringComparison.Ordinal) + 1);

                var reference = new SystemReference(new DirectoryInfo(root), new FileInfo(Path.Combine(root, file)), name);

                var references = locator.Load(root, reference);

                return package.GenerateReferences(references);
            }

            var appName = name + "-glue";
            var appDirectory = new DirectoryInfo(root);
            var appFile = new FileInfo(Path.Combine(root + configuration.ScriptPath, appName));
            var appReference = new SystemReference(appDirectory, appFile, appName) { Type = Model.Reference.TypeEnum.App };

            return package.GenerateReferences(new[] { appReference });
        }

        public static void RewriteContent(
            LoadedConfiguration configuration,
            IReferenceLocator locator,
            IFileCache cache,
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
                                                : Model.Reference.TypeEnum.Dependency;

            var package = Package.Build(configuration, root, cache);
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
                    case Model.Reference.TypeEnum.Library:
                    case Model.Reference.TypeEnum.Module:
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
