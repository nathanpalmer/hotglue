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
        public string Name { get; set; }
        public string Path { get; set; }
        public TypeEnum Type { get; set; }
        public string Extension { get; set; }
        public string Content { get; set; }

        public string FullPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) || !string.IsNullOrWhiteSpace(Path))
            {
                return PT.Combine(PT.Combine(PT.GetFullPath(path), Path.StartsWith("/") ? Path.Substring(1) : Path), Name.StartsWith("/") ? Name.Substring(1) : Name);
            }
            return Name;
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
            if (this.Equals(obj)) return 0;
            return -1;
        }

        public enum TypeEnum
        {
            App,
            Dependency,
            Module
        }
    }
}
