using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HotGlue.Model;

namespace HotGlue
{
    public class DynamicLoading : IReferenceLocator
    {
        private readonly HotGlueConfiguration _config;
        private IGetReferences _getReferences;

        public DynamicLoading(HotGlueConfiguration config, IGetReferences getReferences)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            _config = config;
            _getReferences = getReferences;
        }

        public IEnumerable<Reference> Load(string rootPath, Reference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            //if (String.IsNullOrWhiteSpace(relativePath))
            //{
            //    throw new ArgumentNullException("relativePath");
            //}

            //if (String.IsNullOrWhiteSpace(fileName))
            //{
            //    throw new ArgumentNullException("fileName");
            //}

            var rootIndex = reference.Root.IndexOf(rootPath, StringComparison.OrdinalIgnoreCase);
            var relativePath = reference.Root;
            if (rootIndex >= 0)
            {
                relativePath = relativePath.Substring(rootIndex+rootPath.Length);
                if (relativePath.Length > 1 && relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
                {
                    relativePath = relativePath.Substring(1);
                }
            }

            var results = _getReferences.Parse(_config, rootPath, relativePath, reference.Path);

            if (!results.Any())
            {
                yield return reference;
            }

            // Check for circular reference, if there are any, loading order won't work.
            foreach (var root in results)
            {
                var rootKey = root.Key;
                foreach (var value in root.Value)
                {
                    var subKey = Path.Combine(value.Path, value.Name);
                    foreach (var subResults in results[subKey])
                    {
                        var subPath = Path.Combine(subResults.Path, subResults.Name);
                        if (subPath == rootKey)
                        {
                            throw new Exception(String.Format("Circular reference detected between file '{0}' and '{1}'", rootKey, subKey));
                        }
                    }
                }
            }

            var processed = new List<String>();
            string returnOrder = "";
            var loops = 0;
            var maxLoops = results.Count;
            while (results.Any())
            {
                if (loops++ > maxLoops)
                {
                    throw new StackOverflowException();
                }

                var noDependencies = results.Where(x => x.Value.Count(v => !processed.Contains(Path.Combine(v.Path, v.Name))) == 0)
                    .OrderBy(x => x.Key).ToList();
                if (!noDependencies.Any())
                {
                    throw new Exception("Unable to add order, there aren't any files that don't have a dependency");
                }

                returnOrder += "// " + loops + "\r\n";
                foreach (var noDependency in noDependencies)
                {
                    var path = noDependency.Key.Replace(rootPath, "");
                    var file = path.Substring(path.LastIndexOf("\\") + 1);
                    path = path.Replace(file, "");
                    yield return new Reference
                                 {
                                     Module = false,
                                     Root = path,
                                     Path = file
                                 };
                    returnOrder += noDependency.Key + "\r\n";
                    processed.Add(noDependency.Key);
                    results.Remove(noDependency.Key);
                }
            }

            //return returnOrder;
        }
    }
}