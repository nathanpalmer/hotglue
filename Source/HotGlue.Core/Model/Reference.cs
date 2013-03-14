using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PT = System.IO.Path;

namespace HotGlue.Model
{
    public class Reference : IComparable
    {
        private string _name;
        /// <summary>
        /// The file name (not including relative path)
        /// </summary>
        public string Name
        {
            get { return _name; }
            protected set { _name = value.Reslash(); }
        }

        private string _path;
        /// <summary>
        /// The full relative path
        /// </summary>
        public string Path
        {
            get { return _path; }
            protected set 
            { 
                _path = value.Reslash();
                // Clean relative path so path.combine works
                if (_path.StartsWith("/"))
                {
                    _path = _path.Substring(1);
                }
            }
        }
        
        /// <summary>
        /// The type of file usage when referenced
        /// </summary>
        public TypeEnum Type { get; set; }
        /// <summary>
        /// File extension
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// The files post processing content
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// If the file loading needs to wait on this dependency before continuing
        /// </summary>
        public bool Wait { get; set; }

        public override bool Equals(object obj)
        {
            var reference = obj as Reference;
            if (reference == null) return false;

            return String.Compare(Name, reference.Name) == 0 &&
                   String.Compare(Path, reference.Path) == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 11;
                hash = hash * 7 + Name.GetHashCode();
                hash = hash * 7 + Path.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(object obj)
        {
            if (Equals(obj)) return 0;
            return -1;
        }

        public enum TypeEnum
        {
            /// <summary>
            /// The main file to return with all other dependencies
            /// </summary>
            App,
            /// <summary>
            /// Return file is parsed for additional references
            /// </summary>
            Dependency,
            /// <summary>
            /// Returned file is not parsed for references
            /// </summary>
            Library,
            /// <summary>
            /// Returned file is wrapped as a self contained module
            /// </summary>
            Module,
            /// <summary>
            /// Returned file doesn't exist, is generated dynamically from user request
            /// </summary>
            Generated
        }
    }

    public class RelativeReference : Reference
    {
        /// <summary>
        /// The position in the referring file where the reference was first used.
        /// This is used to ensure that references are loaded in order.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// The text after require referenced in the file
        /// </summary>
        public string ReferenceName { get; private set; }

        public RelativeReference(string referenceName, int index)
        {
            if (String.IsNullOrWhiteSpace(referenceName))
            {
                throw new ArgumentNullException("referenceName");
            }
            // Not resolving with System.File because reference could be anything
            Name = System.IO.Path.GetFileName(referenceName);
            var dotIndex = Name.LastIndexOf(".");
            if (dotIndex > 0)
            {
                Extension = Name.Substring(dotIndex);
            }
            ReferenceName = referenceName;
            Index = index;
        }

        internal void UpdateFromSystemReference(Reference systemReference)
        {
            this.Path = systemReference.Path;
        }
    }

    public class SystemReference : Reference
    {
        /// <summary>
        /// The various alternate reference names
        /// </summary>
        public IList<string> ReferenceNames { get; private set; }
        
        /// <summary>
        /// The full system directory path to the file
        /// </summary>
        public string SystemPath { get; private set; }

        private string Root
        {
            get
            {
                var relativePathIndex = SystemPath.IndexOf(Path, StringComparison.Ordinal);
                return relativePathIndex >= 0 ? SystemPath.Substring(0, relativePathIndex) : SystemPath;
            }
        }

        public string FullPath
        {
            get
            {
                var path = Path.StartsWith("/") ? Path.Substring(1) : Path;
                return PT.Combine(PT.Combine(Root, path), Name);
            }
        }

        public string RelativePath(bool includeVersion)
        {
            return RelativePath(Root, includeVersion);
        }

        public string RelativePath(string root, bool includeVersion)
        {
            Int64 version = Version;
            return "/"
                   + PT.Combine(Path, Name).Replace("\\", "/")
                   + (includeVersion && version > 0 ? "?" + version : "");
        }

        public Int64 Version
        {
            get
            {
                var fullPath = RealFileName(FullPath);
                var file = new FileInfo(fullPath);
                if (file.Exists)
                {
                    return Convert.ToInt64(file.LastWriteTime.ToString("yyyyMMddHHmm"));
                }
                return 0;
            }
        }

        private string RealFileName(string path)
        {
            Func<string, string[], int, int> lastIndexOf = null;
            lastIndexOf = (p, v, i) =>
            {
                var index = p.LastIndexOf(v[i], StringComparison.Ordinal);
                if (index > 0) return index;

                if (i < v.Length - 1)
                {
                    return lastIndexOf(p, v, ++i);
                }

                return 0;
            };

            var extensionIndex = lastIndexOf(path, new[] { "-module", "-require", "-glue", "-app" }, 0);
            if (extensionIndex > 0)
            {
                path = path.Substring(0, extensionIndex);
            }
            return path;
        }
        
        public SystemReference(DirectoryInfo rootDirectory, FileInfo systemFile, string referenceName)
        {
            SystemPath = systemFile.DirectoryName.Reslash();
            Name = systemFile.Name;
            Extension = systemFile.Extension;
            ReferenceNames = new List<string>()
            {
                referenceName
            };
            var index = SystemPath.IndexOf(rootDirectory.FullName.Reslash(), StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                throw new Exception(String.Format("System file '{0}' was not contained in the root directory '{1}'.", systemFile, rootDirectory));
            }
            Path = SystemPath.Substring(index + rootDirectory.FullName.Length); // should be the full relative path
        }

        private SystemReference() {}

        public SystemReference Clone(string suffix)
        {
            var clone = new SystemReference
                        {
                            ReferenceNames = ReferenceNames,
                            SystemPath = SystemPath,
                            Name = Name + suffix,
                            Extension = Extension,
                            Path = Path,
                            Type = Type,
                            Wait = Wait
                        };
            return clone;
        }

        public static SystemReference Build(Reference.TypeEnum type, string fullPath, string rootPath, string keys)
        {
            var name = System.IO.Path.GetFileName(fullPath).RealFileName();
            var directory = System.IO.Path.GetDirectoryName(fullPath);

            var reference = new SystemReference(new DirectoryInfo(rootPath), new FileInfo(System.IO.Path.Combine(directory, name)), name) { Type = type };
            foreach (var key in keys.Split(','))
            {
                reference.ReferenceNames.Add(key);
            }
            return reference;
        }
    }
}
