using System.Collections.Generic;
using System.IO;

namespace HotGlue
{
    public interface ICompile
    {
        List<string> Extensions { get; }
        bool Handles(string Extension);
        string Compile(FileInfo File);
    }
}