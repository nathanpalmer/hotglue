using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;
using SassAndCoffee.Core;
using SassAndCoffee.JavaScript;
using SassAndCoffee.JavaScript.Uglify;
using Yahoo.Yui.Compressor;

namespace HotGlue.Compilers
{
    public class UglifyCompressor : ICompile
    {
        private InstanceProvider<IJavaScriptRuntime> _jsRuntimeProvider;
        private UglifyCompiler _compiler;

        public List<string> Extensions { get; private set; }

        public UglifyCompressor()
        {
            Extensions = new List<string>(new[] { ".js" });
            _jsRuntimeProvider = new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime());
            _compiler = new SassAndCoffee.JavaScript.Uglify.UglifyCompiler(_jsRuntimeProvider);
        }

        public bool Handles(string Extension)
        {
            return Extensions.Where(e => e == Extension).Any();
        }

        public void Compile(ref Reference reference)
        {
            reference.Content = _compiler.Compile(reference.Content);
        }
    }
}
