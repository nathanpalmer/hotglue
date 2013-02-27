using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue
{
    public interface IJavaScriptCompiler
    {
        void LoadLibrary(string code);
        T Execute<T>(string functionName, params object[] args);
    }
}
