using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Console
{
    public class Concatenator
    {
        public static string Compile(string rootPath, string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentNullException("rootPath");
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            var config = HotGlueConfiguration.Default();
            var locator = new GraphReferenceLocator(config);
            var directoryInfo = new DirectoryInfo(rootPath);
            var fileInfo = new FileInfo(Path.Combine(filePath, fileName));
            var reference = new SystemReference(directoryInfo, fileInfo, fileName);
            var references = locator.Load(rootPath, reference);
            var package = Package.Build(config, rootPath);
            return package.Compile(references);
        }
    }
}
