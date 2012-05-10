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
            reference.Extension = ".js";
            reference.Content = @"
var template = jQuery.template(""" + reference.Content.Replace("\r\n", "").Replace("\n", "").Replace("\"", "'") + @""");
module.exports = (function(data){ return jQuery.tmpl(template, data); });";
        }
    }
}
