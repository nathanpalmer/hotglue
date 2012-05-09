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

            var sw = new StringBuilder();
            var modules = false;

            foreach(var reference in references)
            {
                switch(reference.Type)
                {
                    case Reference.TypeEnum.App:
                        if (modules) sw.AppendLine(CompileStitch());
                        sw.AppendLine(CompileDependency(reference));
                        break;
                    case Reference.TypeEnum.Dependency:
                        sw.AppendLine(CompileDependency(reference));
                        break;
                    case Reference.TypeEnum.Module:
                        modules = true;
                        sw.AppendLine(CompileModule(reference));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return sw.ToString();
        }

        public string CompileDependency(Reference reference)
        {
            return File.ReadAllText(reference.FullPath(_relativeRoot));
        }

        public string CompileModule(Reference reference)
        {
            var itemName = reference.Name.ToLower().Replace(reference.Path, "").Replace("\\", "/");
            var item = new FileInfo(reference.FullPath(_relativeRoot));
            if (!string.IsNullOrWhiteSpace(item.Extension))
            {
                itemName = itemName.Replace(item.Extension, "");
            }

            var compiler = _compilers.FirstOrDefault(c => c.Handles(item.Extension));
            if (compiler == null) return null; // Just returning if there isn't a compiler for this handler. Could possibly handle this differently

            return @"if (typeof(__hotglue_assets) === 'undefined') __hotglue_assets = {}; __hotglue_assets['" + itemName + @"'] = function(exports, require, module) {
" + compiler.Compile(item) + @"
}";
        }

        public string CompileStitch()
        {
            return @"
if (typeof(__hotglue_assets) === 'undefined') __hotglue_assets = {};
(function(assets) {
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
}).call(this)(__hotglue_assets)";
        }

        public string References(IEnumerable<Reference> references)
        {
            if (references == null) return "";

            var sw = new StringBuilder();
            var modules = false;

            foreach (var reference in references)
            {
                switch (reference.Type)
                {
                    case Reference.TypeEnum.App:
                        if (modules)
                        {
                            sw.AppendLine(_generateScriptReference.GenerateReference(new Reference
                            {
                                Name = "get.js-require",
                                Type = Reference.TypeEnum.Dependency,
                                Path = ""
                            }));
                        }
                        sw.AppendLine(_generateScriptReference.GenerateReference(reference));
                        break;
                    case Reference.TypeEnum.Dependency:
                        sw.AppendLine(_generateScriptReference.GenerateReference(reference));
                        break;
                    case Reference.TypeEnum.Module:
                        modules = true;
                        sw.AppendLine(_generateScriptReference.GenerateReference(new Reference
                        {
                            Name = reference.Name + "-module",
                            Type = reference.Type,
                            Path = reference.Path
                        }));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return sw.ToString();
        }
    }
}
