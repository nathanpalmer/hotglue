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
    public class CoffeeScriptTests
    {
        [Test]
        public void Can_Compile_CoffeeScript()
        {
            // Arrange
            var compiler = new CoffeeScriptCompiler();
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
    }
}
