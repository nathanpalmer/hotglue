using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HotGlue.Model;

namespace HotGlue
{
    public class GraphReferenceLocator : IReferenceLocator
    {
        private readonly HotGlueConfiguration _config;
        private readonly IEnumerable<IFindReference> _findReferences;

        public GraphReferenceLocator(HotGlueConfiguration config, IEnumerable<IFindReference> findReferences)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (!findReferences.Any())
            {
                throw new ArgumentException("No findReferences found, nothing to parse");
            }
            _config = config;
            _findReferences = findReferences;
        }

        public IEnumerable<Reference> Load(string rootPath, Reference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            // Ensure we have the root path separate from the reference
            var rootIndex = reference.Path.IndexOf(rootPath, StringComparison.OrdinalIgnoreCase);
            var relativePath = reference.Path;
            if (rootIndex >= 0)
            {
                relativePath = relativePath.Substring(rootIndex+rootPath.Length);
                if (relativePath.Length > 1 && relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
                {
                    relativePath = relativePath.Substring(1);
                }
            }

            var results = Parse(_config, rootPath, relativePath, reference.Name);

            if (!results.Any())
            {
                yield return reference;
            }

            CheckForCircularReferences(results);

            // Return the files in the correct order
            var processed = new List<String>();
            var loops = 0;
            var maxLoops = results.Count;
            var fileReferences = results.Values.SelectMany(x => x).ToList();
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

                foreach (var noDependency in noDependencies)
                {
                    var path = noDependency.Key.Replace(rootPath, "");
                    var file = path.Substring(path.LastIndexOf("\\") + 1);
                    path = path.Replace(file, "");
                    yield return new Reference
                                 {
                                     Module = false,
                                     Path = path,
                                     Name = file
                                 };
                    processed.Add(noDependency.Key);
                    results.Remove(noDependency.Key);
                }
            }
        }

        private void CheckForCircularReferences(Dictionary<string, IList<Reference>> references)
        {
            // Check for circular reference, if there are any, loading order won't work.
            foreach (var root in references)
            {
                foreach (var value in root.Value)
                {
                    var subKey = Path.Combine(value.Path, value.Name);
                    foreach (var subResults in references[subKey])
                    {
                        var subPath = Path.Combine(subResults.Path, subResults.Name);
                        if (subPath == root.Key)
                        {
                            throw new Exception(String.Format("Circular reference detected between file '{0}' and '{1}'", root.Key, subKey));
                        }
                    }
                }
            }
        }

        public Dictionary<string, IList<Reference>> Parse(HotGlueConfiguration config, String rootPath, String relativePath, String fileName)
        {
            String currentPath = Path.Combine(rootPath, relativePath);
            String sharedPath = null;
            if (!String.IsNullOrWhiteSpace(config.ScriptSharedFolder))
            {
                sharedPath = Path.Combine(rootPath, config.ScriptSharedFolder);
            }
            var references = new Dictionary<String, IList<Reference>>();
            Parse(currentPath, sharedPath, fileName, references, null);
            return references;
        }

        // recursive function
        private void Parse(String currentPath, String sharedPath, String fileName, Dictionary<String, IList<Reference>> references, Reference parentReference)
        {
            String pathToLookAt = null;
            String fileToLookAt = null;
            var currentFile = Path.Combine(currentPath, fileName);
            if (File.Exists(currentFile))
            {
                pathToLookAt = currentPath;
                fileToLookAt = currentFile;
            }
            else if (!String.IsNullOrWhiteSpace(sharedPath))
            {
                var sharedFile = Path.Combine(sharedPath, fileName);
                if (File.Exists(sharedFile))
                {
                    currentPath = sharedPath; // Once in shared, only look at shared for other models.
                    pathToLookAt = sharedPath;
                    fileToLookAt = sharedFile;
                }
            }

            if (pathToLookAt == null || fileToLookAt == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the file: '{0}' in either the current path: '{1}', or the shared path: '{2}'.", fileName, currentPath, sharedPath));
            }

            var newReferences = HasReferences(pathToLookAt, fileToLookAt, references, parentReference);
            foreach (var fileReference in newReferences)
            {
                Parse(currentPath, sharedPath, fileReference.Name, references, fileReference);
            }
        }

        private IList<Reference> HasReferences(String path, String file, Dictionary<String, IList<Reference>> references, Reference parentReference)
        {
            if (parentReference != null)
            {
                parentReference.Path = path;
            }

            if (references.ContainsKey(file))
            {
                return new List<Reference>(); // already parsed file
            }

            references.Add(file, new List<Reference>());

            var text = File.ReadAllText(file);
            var currentReferences = new List<Reference>();
            foreach (var findReference in _findReferences)
            {
                currentReferences.AddRange(findReference.Parse(text));
            }

            var newReferences = new List<Reference>();
            var list = references[file];
            foreach (var currentReference in currentReferences)
            {
                if (!list.Contains(currentReference))
                {
                    newReferences.Add(currentReference);
                    references[file].Add(currentReference);
                }
            }

            return newReferences;
        }
    }
}