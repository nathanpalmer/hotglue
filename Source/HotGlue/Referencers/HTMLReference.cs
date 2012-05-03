using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class HTMLReference : IReference
    {
        public string GenerateReference(Reference reference)
        {
            return string.Format("<script src=\"{0}\"></script>", Path.Combine(reference.Root,reference.Path));
        }
    }
}