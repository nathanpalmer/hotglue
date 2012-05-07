using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    public interface IReferenceLocator
    {
        IEnumerable<Reference> Load(string relativePath, Reference reference);
    }

    public class DynamicLoading : IReferenceLocator
    {
        private readonly HotGlueConfiguration _config;
        private GetReferences getReferences;

        public DynamicLoading(HotGlueConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            _config = config;
            getReferences = new GetReferences();
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

            var results = getReferences.Parse(_config, rootPath, relativePath, reference.Path);

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

    public class GetFileReferences
    {
        static readonly Regex ReferenceCommentRegex = new Regex(
            @"^\s*(//|\*|#)=\s*require\s*(""|')?(?<path>.+?)(""|')?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        static readonly Regex ReferenceVariableRegex = new Regex(
            @"^\s*var\s+(?<variable>\S+)\s*=\s*require\((""|')?(?<path>.+?)(""|')?\)\s*;?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IList<FileReference> Parse(String fileText)
        {
            var fileReferences = new List<FileReference>();
            if (String.IsNullOrWhiteSpace(fileText))
            {
                return fileReferences;
            }

            var commentMatches = ReferenceCommentRegex.Matches(fileText)
                                    .Cast<Match>()
                                    .Select(m => new FileReference() { Name = m.Groups["path"].Value })
                                    .Where(m => !String.IsNullOrWhiteSpace(m.Name))
                                    .ToList();
            fileReferences.AddRange(commentMatches);

            var variableMatches = ReferenceVariableRegex.Matches(fileText)
                                    .Cast<Match>()
                                    .Select(m => new FileReference() { Name = m.Groups["path"].Value, Variable = m.Groups["variable"].Value, Wrap = true })
                                    .Where(m => !String.IsNullOrWhiteSpace(m.Name) && !String.IsNullOrWhiteSpace(m.Variable))
                                    .ToList();
            fileReferences.AddRange(variableMatches);

            return fileReferences;
        }
    }

    public class FileReference
    {
        private String name;

        public String Name
        {
            get { return name; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    name = "";
                }
                else
                {
                    name = value.Trim();
                }
            }
        }

        private String variable;

        public String Variable
        {
            get { return variable; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    variable = "";
                }
                else
                {
                    variable = value.Trim();
                }
            }
        }

        public String Path { get; set; }

        public Boolean Wrap { get; set; }

        public override bool Equals(object obj)
        {
            var reference = obj as FileReference;
            if (reference == null) return false;

            return String.Compare(name, reference.name) == 0 &&
                   String.Compare(variable, reference.variable) == 0 &&
                   String.Compare(Path, reference.Path) == 0 &&
                   Wrap == reference.Wrap;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 11;
                hash = hash * 7 + name.GetHashCode();
                hash = hash * 7 + variable.GetHashCode();
                hash = hash * 7 + Path.GetHashCode();
                hash = hash * 7 + Wrap.GetHashCode();
                return hash;
            }
        }
    }

    public class GetReferences
    {
        private GetFileReferences _getFileReferences;

        public GetReferences()
        {
            _getFileReferences = new GetFileReferences();
        }

        public Dictionary<String, IList<FileReference>> Parse(HotGlueConfiguration config, String rootPath, String relativePath, String fileName)
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
            var currentReferences = _getFileReferences.Parse(text);

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
