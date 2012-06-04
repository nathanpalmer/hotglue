using HotGlue.Model;

namespace HotGlue
{
    public interface IGenerateScriptReference
    {
        string GenerateReference(SystemReference reference);
    }
}