using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue
{
    public interface IJavaScriptRuntime
    {
        void LoadLibrary(string code);
        string Execute(string functionName, params object[] args);
    }
}
