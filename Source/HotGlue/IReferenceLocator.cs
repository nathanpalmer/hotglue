using System.Collections.Generic;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public interface IReferenceLocator
    {
        IEnumerable<Reference> Load(string relativePath, Reference reference);
    }
}
