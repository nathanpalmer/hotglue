using HotGlue.Model;

namespace HotGlue
{
    public interface IGenerateScriptReference
    {
        string GenerateReference(string root, SystemReference reference);
    }
}