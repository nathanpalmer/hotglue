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
    public class TypeScriptTests
    {
        [Test]
        public void Can_Compile_TypeScript()
        {
            // Arrange
            var runtime = new SassAndCoffeeRuntime();
            var compiler = new TypeScriptCompiler(runtime);
            var reference = new Reference
                {
                    Extension = ".ts",
                    Content = "class Greeter {}"
                };
            var result = "var Greeter = (function () {\n    function Greeter() { }\n    return Greeter;\n})();\n";

            // Act
            compiler.Compile(ref reference);

            // Assert
            reference.Extension.ShouldBe(".js");
            reference.Content.ShouldBe(result);
        }
    }
}
