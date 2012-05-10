using System.Collections.Generic;
using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public interface ICompile
    {
        List<string> Extensions { get; }
        bool Handles(string Extension);
        void Compile(ref Reference reference);
    }
}