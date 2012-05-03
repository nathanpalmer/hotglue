using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public class Package : IPackage
    {
        private readonly string _relativeRoot;
        private readonly IEnumerable<ICompile> _compilers;
        private readonly IReference _reference;

        public Package(string relativeRoot, IEnumerable<ICompile> compilers, IReference reference)
        {
            _relativeRoot = relativeRoot;
            _compilers = compilers;
            _reference = reference;
        }

        public string Compile(IEnumerable<Reference> references)
        {
            if (references == null) return "";
            var refs = references.ToList();

            var sw = new StringWriter();

            var dependencies = refs.Where(x => !x.Module);
            foreach (var dependency in dependencies)
            {
                sw.Write(File.ReadAllText(dependency.FullPath(_relativeRoot)));
            }

            var modules = refs.Where(x => x.Module);

            if (modules.Any())
            {
                sw.Write(@"
(function(/*! Stitch !*/) {
  if (!this.require) {
    var modules = {}, cache = {}, require = function(name, root) {
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
                    var itemName = module.Path.ToLower().Replace(module.Root, "").Replace("\\", "/");
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

            return sw.ToString();
        }

        public string References(IEnumerable<Reference> references)
        {
            var sw = new StringBuilder();

            foreach (var reference in references)
            {
                sw.Append(_reference.GenerateReference(reference));
            }

            return sw.ToString();
        }
    }
}
