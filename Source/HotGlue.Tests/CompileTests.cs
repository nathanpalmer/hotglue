using System;
using System.Collections.Generic;
using System.IO;
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
            var compilers = new[] { new JQueryTemplateCompiler()  };
            var referencer = new HTMLGenerateScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new SystemReference(new DirectoryInfo("C:/Root"), new FileInfo("C:/Root/Scripts/Compile1/dep1.js"), "dep1.js")
                    {
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
            var compilers = new[] { new JQueryTemplateCompiler() };
            var referencer = new HTMLGenerateScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new SystemReference(new DirectoryInfo("C:/Root"), new FileInfo("C:/Root/Scripts/Compile2/module1.js"), "module1.js")
                    {
                        Type = Reference.TypeEnum.Module
                    }
                };

            // Act
            var result = package.Compile(references);

            // Assert
            result.ShouldBe(@"if(typeof(__hotglue_assets)==='undefined'){__hotglue_assets={};}__hotglue_assets['module1.js'] = { keys: [ 'module1.js' ], item: function(exports, require, module) {var j = 1;}};
");
        }
    }
}
