using System.Collections.Generic;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public interface IReferenceLocator
    {
        IEnumerable<SystemReference> Load(string relativePath, SystemReference reference);
    }
}
