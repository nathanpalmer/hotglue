using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Compilers;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class CompileTests
    {
        [Test]
        public void Should_Compile_Deps_Without_Wrapper()
        {
            // Arrange
            var compilers = new[] { new JavaScriptCompiler() };
            var referencer = new HTMLGenerateScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new Reference
                        {
                            Path = "/Scripts/",
                            Name = "dep1.js",
                            Type = Reference.TypeEnum.Dependency
                        }
                };

            // Act
            var result = package.Compile(references);

            // Assert
            result.ShouldBe(@"var j = 1;
");
        }

        [Test]
        public void Should_Compile_Modules_With_Wrapper()
        {
            // Arrange
            var compilers = new[] { new JavaScriptCompiler() };
            var referencer = new HTMLGenerateScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new Reference
                        {
                            Path = "/Scripts/",
                            Name = "module1.js",
                            Type = Reference.TypeEnum.Module
                        }
                };

            // Act
            var result = package.Compile(references);

            // Assert
            result.ShouldBe(@"
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
""module1"": function(exports, require, module) {
var j = 1;
}
});
");
        }
    }
}
