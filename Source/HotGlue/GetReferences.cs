using System;
using System.Collections.Generic;
using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class GetReferences : IGetReferences
    {
        private readonly IEnumerable<IFindReference> _findReferences;

        public GetReferences(IEnumerable<IFindReference> findReferences)
        {
            _findReferences = findReferences;
        }

        public Dictionary<string, IList<FileReference>> Parse(HotGlueConfiguration config, String rootPath, String relativePath, String fileName)
        {
            String currentPath = Path.Combine(rootPath, relativePath);
            String sharedPath = null;
            if (!String.IsNullOrWhiteSpace(config.ScriptSharedFolder))
            {
                sharedPath = Path.Combine(rootPath, config.ScriptSharedFolder);
            }
            var references = new Dictionary<String, IList<FileReference>>();
            Parse(currentPath, sharedPath, fileName, references, null);
            return references;
        }

        // circular function
        private void Parse(String currentPath, String sharedPath, String fileName, Dictionary<String, IList<FileReference>> references, FileReference parentReference)
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

        private IList<FileReference> HasReferences(String path, String file, Dictionary<String, IList<FileReference>> references, FileReference parentReference)
        {
            if (parentReference != null)
            {
                parentReference.Path = path;
            }

            if (references.ContainsKey(file))
            {
                return new List<FileReference>(); // already parsed file
            }
            references.Add(file, new List<FileReference>());

            var text = File.ReadAllText(file);
            var currentReferences = new List<FileReference>();
            foreach (var findReference in _findReferences)
            {
                currentReferences.AddRange(findReference.Parse(text));
            }

            var newReferences = new List<FileReference>();
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