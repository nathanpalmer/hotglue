using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;
using Yahoo.Yui.Compressor;

namespace HotGlue.Compilers
{
    public class YUICompressor : ICompile
    {
        public List<string> Extensions { get; private set; }

        public YUICompressor()
        {
            Extensions = new List<string>(new[] { ".js" });
        }

        public bool Handles(string Extension)
        {
            return Extensions.Where(e => e == Extension).Any();
        }

        public void Compile(ref Reference reference)
        {
            if (string.IsNullOrWhiteSpace(reference.Content)) return;
            reference.Content = JavaScriptCompressor.Compress(reference.Content);
        }
    }
}
