using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue
{
    public class Package : IPackage
    {
        private readonly string _relativeRoot;
        private readonly IEnumerable<ICompile> _compilers;
        private readonly IGenerateScriptReference _generateScriptReference;

        public Package(string relativeRoot, IEnumerable<ICompile> compilers, IGenerateScriptReference generateScriptReference)
        {
            _relativeRoot = relativeRoot;
            _compilers = compilers;
            _generateScriptReference = generateScriptReference;
        }

        public static Package Build(HotGlueConfiguration configuration)
        {
            IGenerateScriptReference generateScriptReference;
            if (configuration == null || string.IsNullOrWhiteSpace(configuration.Referencer))
            {
                generateScriptReference = new HTMLGenerateScriptReference();
            }
            else
            {
                generateScriptReference = (IGenerateScriptReference)Activator.CreateInstance(Type.GetType(configuration.Referencer));
            }

            IEnumerable<ICompile> compilers;
            if (configuration == null || configuration.Compilers == null || configuration.Compilers.Length == 0)
            {
                compilers = new[]
                    {
                        new JavaScriptCompiler()
                    };
            }
            else
            {
                compilers = configuration.Compilers.Select(compiler => (ICompile)Activator.CreateInstance(Type.GetType(compiler.Type))).ToList();
            }

            var package = new Package("", compilers, generateScriptReference);
            return package;
        }

        public string Compile(IEnumerable<Reference> references)
        {
            if (references == null) return "";
            var refs = references.ToList();

            var sw = new StringWriter();

            var modules = refs.Where(x => x.Module);

            if (modules.Any())
            {
                sw.Write(@"
(function(/*! Stitch !*/) {
  if (!this.require) {
    var modules = {}, cache = {}, require = function(name, root) {
      name = name.replace(/.js/,'');
      var module = cache[name], path = expand(root, name), fn;
      if (module) {
        return module;
      } else if (fn = modules[path] || modules[path = expand(path, './index')]) {
        module = {id: name, exports: {}};
        try {
          cache[name] = module.exports;
          fn(module.exports, function(name) {
            return require(name, dirname(path));
          }, module);
          return cache[name] = module.exports;
        } catch (err) {
          delete cache[name];
          throw err;
        }
      } else {
        throw 'module \'' + name + '\' not found';
      }
    }, expand = function(root, name) {
      var results = [], parts, part;
      if (/^\.\.?(\/|$)/.test(name)) {
        parts = [root, name].join('/').split('/');
      } else {
        parts = name.split('/');
      }
      for (var i = 0, length = parts.length; i < length; i++) {
        part = parts[i];
        if (part == '..') {
          results.pop();
        } else if (part != '.' && part != '') {
          results.push(part);
        }
      }
      return results.join('/');
    }, dirname = function(path) {
      return path.split('/').slice(0, -1).join('/');
    };
    this.require = function(name) {
      return require(name, '');
    }
    this.require.define = function(bundle) {
      for (var key in bundle)
        modules[key] = bundle[key];
    };
  }
  return this.require.define;
}).call(this)({
");

                var i = 0;
                foreach (var module in modules)
                {
                    var itemName = module.Name.ToLower().Replace(module.Path, "").Replace("\\", "/");
                    var item = new FileInfo(module.FullPath(_relativeRoot));
                    if (!string.IsNullOrWhiteSpace(item.Extension))
                    {
                        itemName = itemName.Replace(item.Extension, "");
                    }
                    sw.Write(i == 0 ? "" : ", ");
                    sw.Write(string.Format("\"{0}\"", itemName));
                    sw.Write(": function(exports, require, module) ");

                    var compiler = _compilers.FirstOrDefault(c => c.Handles(item.Extension));
                    if (compiler == null) return null; // Just returning if there isn't a compiler for this handler. Could possibly handle this differently

                    sw.Write("{" + Environment.NewLine + compiler.Compile(item) + Environment.NewLine + "}" + Environment.NewLine);

                    i++;
                }

                sw.Write("});" + Environment.NewLine);
            }

            var dependencies = refs.Where(x => !x.Module);
            foreach (var dependency in dependencies)
            {
                sw.WriteLine(File.ReadAllText(dependency.FullPath(_relativeRoot)));
            }

            return sw.ToString();
        }

        public string References(IEnumerable<Reference> references)
        {
            var sw = new StringBuilder();

            foreach (var reference in references)
            {
                sw.Append(_generateScriptReference.GenerateReference(reference));
            }

            return sw.ToString();
        }
    }
}
