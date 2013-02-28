using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jurassic;

namespace HotGlue.Runtimes
{
    public class JurassicRuntime : IJavaScriptRuntime
    {
        private readonly ScriptEngine _engine;

        public JurassicRuntime()
        {
            _engine = new ScriptEngine();
        }

        public void LoadLibrary(string code)
        {
            _engine.Evaluate(code);
        }

        public string Execute(string functionName, params object[] args)
        {
            return _engine.CallGlobalFunction<String>(functionName, args);
        }
    }
}
