using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PT = System.IO.Path;

namespace HotGlue.Model
{
    public class Reference
    {
        public string Root { get; set; }
        public string Path { get; set; }
        public bool Module { get; set; }

        public string FullPath(string root)
        {
            if (!string.IsNullOrWhiteSpace(root) || !string.IsNullOrWhiteSpace(Root))
            {
                return PT.Combine(PT.Combine(PT.GetFullPath(root), Root.StartsWith("/") ? Root.Substring(1) : Root), Path.StartsWith("/") ? Path.Substring(1) : Path);
            }
            return Path;
        }
    }
}
