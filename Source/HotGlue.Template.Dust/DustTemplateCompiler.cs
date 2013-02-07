using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Compilers
{
    public class DustTemplateCompiler : ICompile
    {
        public List<string> Extensions { get; private set; }

        public DustTemplateCompiler()
        {
            Extensions = new List<string>(new[] { ".tmpl", ".tmpl-dust" });
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
var compiled = dust.compileFn(template, """ + name + @""");
module.exports = (function(data,callback){
    compiled(data, callback);
});";
        }
    }
}
