using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    public class GraphReferenceLocator : IReferenceLocator
    {
        private readonly LoadedConfiguration _configuration;

        public GraphReferenceLocator(LoadedConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            _configuration = configuration;
        }

        public IEnumerable<SystemReference> Load(string rootPath, SystemReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            rootPath = rootPath.Reslash();
            // Ensure we have the root path separate from the reference
            var rootIndex = reference.Path.IndexOf(rootPath, StringComparison.OrdinalIgnoreCase);
            var relativePath = reference.Path;
            if (rootIndex >= 0)
            {
                relativePath = relativePath.Substring(rootIndex+rootPath.Length);
            }
            
            var results = Parse(rootPath, relativePath, reference.Name);

            if (!results.Any())
            {
                yield return reference;//.ToSystemReference(rootPath);
            }

            CheckForCircularReferences(results);

            // Return the files in the correct order
            var processed = new List<Reference>();
            var loops = 0;
            var maxLoops = results.Count;
            while (results.Any())
            {
                if (loops++ > maxLoops)
                {
                    throw new StackOverflowException();
                }

                var noDependencies = results
                    .Where(x => x.Value.Count(v => !processed.Contains(v)) == 0)
                    .ToList();

                if (!noDependencies.Any())
                {
                    throw new Exception("Unable to add order, there aren't any files that don't have a dependency");
                }

                for (int index = 0; index < noDependencies.Count; index++)
                {
                    var noDependency = noDependencies[index];
                    
                    processed.Add(noDependency.Key);
                    results.Remove(noDependency.Key);
                    if (index == noDependencies.Count - 1)
                    {
                        noDependency.Key.Wait = true;
                    }
                    yield return noDependency.Key;
                }
            }
        }

        private void CheckForCircularReferences(Dictionary<SystemReference, IList<RelativeReference>> references)
        {
            var baseReferences = references.ToDictionary(k => (Reference) k.Key, v => (IList<Reference>)v.Value.Cast<Reference>().ToList());
            // Check for circular reference, if there are any, loading order won't work.
            Action<Reference, KeyValuePair<Reference, IList<Reference>>> compareChildren = null;
            compareChildren = (reference, list) =>
                {
                    foreach (var childReference in list.Value)
                    {
                        if (childReference.Equals(reference))
                        {
                            throw new Exception(String.Format("Circular reference detected between file '{0}' and '{1}'", Path.Combine(reference.Path, reference.Name), Path.Combine(list.Key.Path, list.Key.Name)));
                        }
                        if (baseReferences.ContainsKey(childReference))
                        {
                            compareChildren(reference, baseReferences.Single(x => x.Key.Equals(childReference)));
                        }
                    }
                };

            foreach (var root in baseReferences.Where(root => root.Value.Any()))
            {
                compareChildren(root.Key, new KeyValuePair<Reference, IList<Reference>>(root.Key, root.Value.Cast<Reference>().ToList()));
            }
        }

        private Dictionary<SystemReference, IList<RelativeReference>> Parse(String rootPath, String relativePath, String fileName)
        {
            var rootDirectory = new DirectoryInfo(rootPath);
            if (!rootDirectory.Exists)
            {
                throw new DirectoryNotFoundException(String.Format("The rootPath '{0}' passed in doesn't exist or can't resolve.", rootPath));
            }
            var references = new Dictionary<SystemReference, IList<RelativeReference>>();
            Parse(rootDirectory, relativePath, new RelativeReference(fileName) { Type = Reference.TypeEnum.App }, ref references);
            AddAbsoluteReferences(references);
            return references;
        }

        private void AddAbsoluteReferences(IDictionary<SystemReference, IList<RelativeReference>> references)
        {
            foreach (var systemReference in references)
            {
                
                var absoluteReference = systemReference.Key.RelativePath(false);
                if (!systemReference.Key.ReferenceNames.Contains(absoluteReference,
                                                                 StringComparer.InvariantCultureIgnoreCase))
                {
                    systemReference.Key.ReferenceNames.Add(absoluteReference);
                }
            }
        }

        // recursive function
        private void Parse(DirectoryInfo rootDirectory, String relativePath, RelativeReference relativeReference, ref Dictionary<SystemReference, IList<RelativeReference>> references)
        {
            String currentPath = Path.Combine(rootDirectory.FullName, relativePath);
            SystemReference systemReference = null;
            var modifiedPath = Regex.Replace(relativeReference.ReferenceName, "^~/", "/");
            var referenceIsAbsolutePath = !string.IsNullOrEmpty(relativeReference.ReferenceName) &&
                                          (modifiedPath[0] == '/');
            var filePath = referenceIsAbsolutePath
                               ? Path.Combine(rootDirectory.FullName, modifiedPath.Substring(1))
                               : Path.Combine(currentPath, relativeReference.ReferenceName);
            var fileReference = new FileInfo(filePath);
            if (fileReference.Exists)
            {
                systemReference = new SystemReference(rootDirectory, fileReference, relativeReference.ReferenceName) { Type = relativeReference.Type };
            }

            if (systemReference == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the file: '{0}' in the current path: '{1}'.", relativeReference.Name, filePath));
            }

            relativeReference.UpdateFromSystemReference(systemReference);
            // We check for library references here rather than below, because we have the actual file path now
            if (relativeReference.Type == Reference.TypeEnum.Library)
            {
                if (!references.ContainsKey(systemReference))
                {
                    references.Add(systemReference, new List<RelativeReference>());   
                }
                return;
            }

            var newRelativeReferences = GetReferences(rootDirectory, systemReference, ref references);
            foreach (var reference in newRelativeReferences)
            {
                Parse(rootDirectory, systemReference.Path, reference, ref references);
            }
        }

        private IList<RelativeReference> GetReferences(DirectoryInfo rootDirectory, SystemReference reference, ref Dictionary<SystemReference, IList<RelativeReference>> references)
        {
            if (references.ContainsKey(reference))
            {
                // Duplicates within the different files with different types
                CheckForDuplicateReference(reference, references.Keys);
                // if its not a duplicate reference type, but has a different reference name, add it to the collection
                var existing = references.Single(x => x.Key.Equals(reference));
                if (!existing.Key.ReferenceNames.Contains(reference.ReferenceNames.Single()))
                {
                    existing.Key.ReferenceNames.Add(reference.ReferenceNames.Single());
                }
                return new List<RelativeReference>(); // already parsed file
            }

            references.Add(reference, new List<RelativeReference>());

            var text = File.ReadAllText(reference.FullPath);
            var currentReferences = new List<RelativeReference>();
            foreach (var findReference in _configuration.FindReferences)
            {
                currentReferences.AddRange(findReference.Parse(text));
            }

            var list = references[reference];
            foreach (var currentReference in currentReferences)
            {
                // find any duplicate references for the same type in the file and remove.
                // look off the full referenced name and not the internal equals name
                if (!list.Any(x => x.ReferenceName.Equals(currentReference.ReferenceName, StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add(currentReference);
                }
                else
                {
                    // Duplicates within the same file with different types
                    CheckForDuplicateReference(currentReference, list);
                }
            }

            return list;
        }

        private void CheckForDuplicateReference(Reference reference, IEnumerable<Reference> references)
        {
            var existing = references.Single(x => x.Equals(reference));
            if ((existing.Type == Reference.TypeEnum.Module && (reference.Type == Reference.TypeEnum.App || reference.Type == Reference.TypeEnum.Dependency) ||
                (existing.Type == Reference.TypeEnum.App || existing.Type == Reference.TypeEnum.Dependency) && reference.Type == Reference.TypeEnum.Module))
            {
                throw new Exception(String.Format("A different require reference was found for the file: '{0}'. You can only have //=requires or var variable = require('') for all references to the same file.", Path.Combine(reference.Path ?? "", reference.Name)));
            }
        }
    }
}