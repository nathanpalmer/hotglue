using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Compilers
{
    public class JsRenderTemplateCompiler : ICompile
    {
        public List<string> Extensions { get; private set; }

        public JsRenderTemplateCompiler()
        {
            Extensions = new List<string>(new[] { ".tmpl", ".tmpl-jsrender" });
        }

        public bool Handles(string Extension)
        {
            return Extensions.Any(e => e == Extension);
        }

        public void Compile<T>(ref T reference) where T : Reference
        {
            var name = reference.Name;
            reference.Extension = ".js";
            reference.Content = @"
var template = """ + reference.Content.Replace("\r\n", "").Replace("\n", "").Replace("\"", "'") + @""";
var compiled = $.templates(""" + name + @""", template);
module.exports = (function(data){ return compiled.render(data); });";
        }
    }
}
