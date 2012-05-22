using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EcmaScript.NET;
using HotGlue.Model;
using SassAndCoffee.Core;
using SassAndCoffee.Core.Caching;
using SassAndCoffee.JavaScript;
using SassAndCoffee.JavaScript.CoffeeScript;
using SassAndCoffee.JavaScript.JavaScriptEngines;

namespace HotGlue.Compilers
{
    public class CoffeeScriptCompiler : ICompile
    {
        private IInstanceProvider<IJavaScriptRuntime> _jsRuntimeProvider;
        private SassAndCoffee.JavaScript.CoffeeScript.CoffeeScriptCompiler _compiler;
        public List<string> Extensions { get; private set; }

        public CoffeeScriptCompiler()
        {
            Extensions = new List<string>(new[] { ".coffee" });

            _jsRuntimeProvider = new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime());
            _compiler = new SassAndCoffee.JavaScript.CoffeeScript.CoffeeScriptCompiler(_jsRuntimeProvider);
        }

        public bool Handles(string Extension)
        {
            return Extensions.Any(e => e == Extension);
        }

        public void Compile(ref Reference reference)
        {
            reference.Extension = ".js";
            try
            {
                reference.Content = _compiler.Compile(reference.Content);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var line in ex.Message.Split(new[]{"\n"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine(string.Format("console.error(\"{0}\");", line));
                }
                reference.Content = sb.ToString();
            }
            
        }
    }
}
