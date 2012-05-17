using HotGlue.Compilers;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class ReferenceTests
    {
        [Test]
        public void Should_Generate_HTML_References()
        {
            // Arrange
            var compilers = new[] { new JQueryTemplateCompiler(),  };
            var referencer = new HTMLGenerateScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new Reference
                        {
                            Path = "/Scripts/",
                            Name = "depper1.js",
                            Type = Reference.TypeEnum.App
                        }
                };

            // Act
            var result = package.References(references);

            // Assert
            result.ShouldBe(@"<script src=""/Scripts/depper1.js""></script>
");
        }
    }
}