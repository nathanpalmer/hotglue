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

        public static Package Build(HotGlueConfiguration configuration, string root)
        {
            IGenerateScriptReference generateScriptReference;
            if (configuration == null || string.IsNullOrWhiteSpace(configuration.GenerateScript))
            {
                generateScriptReference = new HTMLGenerateScriptReference();
            }
            else
            {
                generateScriptReference = (IGenerateScriptReference)Activator.CreateInstance(Type.GetType(configuration.GenerateScript));
            }

            IList<ICompile> compilers;
            if (configuration == null || configuration.Compilers == null || configuration.Compilers.Length == 0)
            {
                compilers = new[]
                    {
                        new JQueryTemplateCompiler()
                    };
            }
            else
            {
                compilers = configuration.Compilers
                    .Where(c => string.IsNullOrWhiteSpace(c.Mode) || c.Mode.Equals(configuration.Debug ? "debug" : "release", StringComparison.OrdinalIgnoreCase))
                    .Select(compiler => (ICompile)Activator.CreateInstance(Type.GetType(compiler.Type)))
                    .ToList();
            }

            var package = new Package(root, compilers, generateScriptReference);
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
            if (reference.Content == null)
            {
                reference.Content = File.ReadAllText(reference.FullPath(_relativeRoot));
            }
            foreach(var compiler in _compilers)
            {
                if (compiler.Handles(reference.Extension))
                {
                    compiler.Compile(ref reference);
                }
            }
            return reference.Content;
        }
        
        public string CompileModule(Reference reference)
        {
            var itemName = reference.Name.ToLower().Replace(reference.Path, "").Replace("\\", "/");

            var sb = new StringBuilder();
            sb.Append(@"if(typeof(__hotglue_assets)==='undefined'){__hotglue_assets={};}__hotglue_assets['" + itemName + @"'] = function(exports, require, module) {");
            sb.Append(CompileDependency(reference));
            sb.Append("}");

            return sb.ToString();
        }

        public string CompileStitch()
        {
            var content = @"
if (typeof(__hotglue_assets) === 'undefined') __hotglue_assets = {};
(function(assets) {
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
}).call(this)(__hotglue_assets)";

            
            return CompileDependency(new Reference { Extension = ".js", Content = content });
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
