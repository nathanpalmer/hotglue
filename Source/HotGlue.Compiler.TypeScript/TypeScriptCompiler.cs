using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Model;
using SassAndCoffee.Core;
using SassAndCoffee.Core.Caching;
using SassAndCoffee.JavaScript;
using SassAndCoffee.JavaScript.CoffeeScript;
using SassAndCoffee.JavaScript.JavaScriptEngines;

namespace HotGlue.Compilers
{
    public class TypeScriptCompiler : ICompile
    {
        private IInstanceProvider<SassAndCoffee.JavaScript.IJavaScriptRuntime> _jsRuntimeProvider;
        private SassAndCoffee.JavaScript.TypeScript.TypeScriptCompiler _compiler;
        public List<string> Extensions { get; private set; }

        public TypeScriptCompiler()
        {
            string[] names = this.GetType().Assembly.GetManifestResourceNames();
            var resourceStream = this.GetType().Assembly.GetManifestResourceStream(typeof(SassAndCoffee.JavaScript.TypeScript.TypeScriptCompiler), "typescript.js");
            Extensions = new List<string>(new[] { ".ts" });

            _jsRuntimeProvider = new InstanceProvider<SassAndCoffee.JavaScript.IJavaScriptRuntime>(() => new IEJavaScriptRuntime());
            _compiler = new SassAndCoffee.JavaScript.TypeScript.TypeScriptCompiler(_jsRuntimeProvider);
        }

        public bool Handles(string Extension)
        {
            return Extensions.Any(e => e == Extension);
        }

        public void Compile<T>(ref T reference) where T : Reference
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
