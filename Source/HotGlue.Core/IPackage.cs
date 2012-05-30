using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IPackage
    {
        string Compile(IEnumerable<Reference> references);
        string References(IEnumerable<Reference> references);
        string CompileDependency(Reference reference);
        string CompileModule(Reference reference, string name = null);
        string CompileStitch();
    }
}