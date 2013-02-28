using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Compilers;
using HotGlue.Model;
using HotGlue.Runtimes;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class CoffeeScriptTests
    {
        [Test]
        public void Can_Compile_CoffeeScript()
        {
            // Arrange
            var runtime = new SassAndCoffeeRuntime();
            var compiler = new CoffeeScriptCompiler(runtime);
            var reference = new Reference
                {
                    Extension = ".coffee",
                    Content = "t = 4"
                };
            var result = "var t;\n\nt = 4;\n";

            // Act
            compiler.Compile(ref reference);

            // Assert
            reference.Extension.ShouldBe(".js");
            reference.Content.ShouldBe(result);
        }

        [Test]
        public void Can_Compile_CoffeeScript_WithoutBare()
        {
            // Arrange
            var runtime = new SassAndCoffeeRuntime();
            var compiler = new CoffeeScriptCompiler(runtime);
            compiler.Bare = false;
            var reference = new Reference
            {
                Extension = ".coffee",
                Content = "t = 4"
            };
            var result = "(function() {\n  var t;\n\n  t = 4;\n\n}).call(this);\n";

            // Act
            compiler.Compile(ref reference);

            // Assert
            reference.Extension.ShouldBe(".js");
            reference.Content.ShouldBe(result);
        }
    }
}
