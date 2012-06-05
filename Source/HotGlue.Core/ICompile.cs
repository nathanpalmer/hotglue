using System.Collections.Generic;
using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public interface ICompile
    {
        List<string> Extensions { get; }
        bool Handles(string Extension);
        void Compile<T>(ref T reference) where T : Reference;
    }
}