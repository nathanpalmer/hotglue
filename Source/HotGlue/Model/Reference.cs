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
            set { _name = value.Reslash(); }
        }

        private string _path;
        /// <summary>
        /// The full relative path
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value.Reslash(); }
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

        public Reference()
        {
            
        }

        public string FullPath(string path)
        {
            path = !string.IsNullOrWhiteSpace(path) ? PT.GetFullPath(path.Reslash()) : "";

            if (!string.IsNullOrWhiteSpace(Path))
            {
                path = PT.Combine(path, Path.StartsWith("/") ? Path.Substring(1) : Path);
            }

            path = PT.Combine(path, Name.StartsWith("/") ? Name.Substring(1) : Name);
            
            return path;
        }

        public string RelativePath()
        {
            return RelativePath("", false);
        }

        public string RelativePath(string root, bool includeVersion)
        {
            Int64 version = Version(root);
            return PT.Combine(Path, Name).Replace("\\", "/")
                   + (includeVersion && version > 0 ? "?" + version : "");
        }

        public Int64 Version(string path)
        {
            var fullPath = RealFileName(FullPath(path));
            var file = new FileInfo(fullPath);
            if (file.Exists)
            {
                return Convert.ToInt64(file.LastWriteTime.ToString("yyyyMMddHHmm"));
            }
            return 0;
        }

        private string RealFileName(string path)
        {
            Func<string, string[], int, int> lastIndexOf = null;
            lastIndexOf = (p,v,i) =>
                {
                    var index = p.LastIndexOf(v[i], StringComparison.Ordinal);
                    if (index > 0) return index;

                    if (i < v.Length - 1)
                    {
                        return lastIndexOf(p, v, ++i);
                    }

                    return 0;
                };

            var extensionIndex = lastIndexOf(path, new[] { "-module", "-require", "-glue" }, 0);
            if (extensionIndex > 0)
            {
                path = path.Substring(0, extensionIndex);
            }
            return path;
        }

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

        public SystemReference ToSystemReference(string rootPath)
        {
            return new SystemReference(new DirectoryInfo(rootPath), new FileInfo(FullPath(rootPath)), Name)
                   {
                       Type = Type
                   };
        }

        public enum TypeEnum
        {
            App,
            Dependency,
            Library,
            Module
        }
    }

    public class RelativeReference : Reference
    {
        /// <summary>
        /// The text after require referenced in the file
        /// </summary>
        public string ReferenceName { get; private set; }

        public RelativeReference(string referenceName)
        {
            if (String.IsNullOrWhiteSpace(referenceName))
            {
                throw new ArgumentNullException("referenceName");
            }
            // Not resolving with System.File because reference could be anything
            Name = System.IO.Path.GetFileName(referenceName);
            var index = Name.LastIndexOf(".");
            if (index > 0)
            {
                Extension = Name.Substring(index);
            }
            ReferenceName = referenceName;
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

        public new string FullPath
        {
            get
            {
                var path = Path.StartsWith("/") ? Path.Substring(1) : Path;
                return PT.Combine(PT.Combine(Root, path), Name);
            }
        }

        public new string RelativePath()
        {
            return RelativePath(false);
        }

        public string RelativePath(bool includeVersion)
        {
            return base.RelativePath(Root, includeVersion);
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

        public SystemReference Clone()
        {
            var clone = new SystemReference
                        {
                            ReferenceNames = ReferenceNames,
                            SystemPath = SystemPath,
                            Name = Name,
                            Extension = Extension,
                            Path = Path,
                            Type = Type,
                            Wait = Wait
                        };
            return clone;
        }
    }
}
