using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Compilers
{
    public class JQueryTemplateCompiler : ICompile
    {
        public List<string> Extensions { get; private set; }

        public JQueryTemplateCompiler()
        {
            Extensions = new List<string>(new[] { ".tmpl" });
        }

        public bool Handles(string Extension)
        {
            return Extensions.Any(e => e == Extension);
        }

        public void Compile(ref Reference reference)
        {
            var name = reference.Name;
            reference.Extension = ".js";
            reference.Content = @"
var template = """ + reference.Content.Replace("\r\n", "").Replace("\n", "").Replace("\"", "'") + @""";
jQuery.template(""" + name + @""", template);
module.exports = (function(data){ return jQuery.tmpl(template, data); });";
        }
    }
}
