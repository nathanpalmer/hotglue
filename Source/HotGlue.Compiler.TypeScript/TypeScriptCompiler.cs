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
        public List<string> Extensions { get; private set; }

        private readonly IJavaScriptRuntime _javaScriptRuntime;
        private readonly object _padLock = new object();
        private bool _initialized;

        public TypeScriptCompiler(IJavaScriptRuntime javaScriptRuntime)
        {
            _javaScriptRuntime = javaScriptRuntime;
            Extensions = new List<string>(new[] { ".ts" });
        }

        private void Initialize()
        {
            if (_initialized) return;
            lock (_padLock)
            {
                var library = new StringBuilder();
                var content = GetType().GetResource("HotGlue.Compilers.TypeScript.typescript.js");
                library.Append(content);
                _javaScriptRuntime.LoadLibrary(library.ToString());
                _initialized = true;
            }
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
                Initialize();
                reference.Content = _javaScriptRuntime.Execute("hotglue_compile", reference.Content);
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
