using HotGlue.Model;

namespace HotGlue
{
    public interface IGenerateScriptReference
    {
        string GenerateHeader();
        string GenerateReference(SystemReference reference);
        string GenerateFooter();
    }
}