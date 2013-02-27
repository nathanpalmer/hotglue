using System;
using System.Collections.Generic;
using System.IO;
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
    public class SassAndCoffeeCompiler : IJavaScriptCompiler
    {
        private readonly IJavaScriptRuntime _javaScriptRuntime;

        public SassAndCoffeeCompiler(IJavaScriptRuntime javaScriptRuntime)
        {
            _javaScriptRuntime = javaScriptRuntime;
            _javaScriptRuntime.Initialize();
        }

        public void LoadLibrary(string code)
        {
            _javaScriptRuntime.LoadLibrary(code);
        }

        public T Execute<T>(string functionName, params object[] args)
        {
            return _javaScriptRuntime.ExecuteFunction<T>(functionName, args);
        }
    }

    public class CoffeeScriptCompiler : ICompile
    {
        public List<string> Extensions { get; private set; }

        private readonly IJavaScriptCompiler _javaScriptRuntime;
        private readonly object _padLock = new object();
        private bool _initialized;

        public CoffeeScriptCompiler()
        {
            Extensions = new List<string>(new[] { ".coffee" });

            try
            {
                _javaScriptRuntime = new SassAndCoffeeCompiler(new IEJavaScriptRuntime());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private void Initialize()
        {
            if (_initialized) return;
            lock (_padLock)
            {
                var library = new StringBuilder();
                var content = GetType().GetResource("HotGlue.Compilers.CoffeeScript.coffee-script.js");
                library.Append(content);
                library.Append(@"
function hotglue_compile(code) {
    return CoffeeScript.compile(code);
}
");
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
                reference.Content = _javaScriptRuntime.Execute<String>("hotglue_compile", reference.Content);
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
