using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Compilers
{
    public class CoffeeScriptCompiler : ICompile
    {
        public List<string> Extensions { get; private set; }
        public bool Bare { get; set; }

        private readonly IJavaScriptRuntime _javaScriptRuntime;
        private readonly object _padLock = new object();
        private bool _initialized;

        public CoffeeScriptCompiler(IJavaScriptRuntime javaScriptRuntime)
        {
            Extensions = new List<string>(new[] { ".coffee" });
            Bare = true;

            _javaScriptRuntime = javaScriptRuntime;
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
var root = this;

function hotglue_compile(code, bare) {
    return root.CoffeeScript.compile(code, { bare: bare });
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
                reference.Content = _javaScriptRuntime.Execute("hotglue_compile", reference.Content, Bare);
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
