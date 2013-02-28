using System;
using SassAndCoffee.JavaScript;

namespace HotGlue.Runtimes
{
    public class SassAndCoffeeRuntime : IJavaScriptRuntime
    {
        private readonly SassAndCoffee.JavaScript.IJavaScriptRuntime _javaScriptRuntime;

        public SassAndCoffeeRuntime()
        {
            _javaScriptRuntime = new IEJavaScriptRuntime();
            _javaScriptRuntime.Initialize();
        }

        public void LoadLibrary(string code)
        {
            _javaScriptRuntime.LoadLibrary(code);
        }

        public string Execute(string functionName, params object[] args)
        {
            return _javaScriptRuntime.ExecuteFunction<String>(functionName, args);
        }
    }
}