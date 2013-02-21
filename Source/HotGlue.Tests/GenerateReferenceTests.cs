using System.IO;
using HotGlue.Compilers;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class GenerateReferenceTests
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
                    new SystemReference(new DirectoryInfo("C:/Root/"), new FileInfo("C:/Root/Scripts/depper1.js"), "depper1.js")
                    {
                        Type = Reference.TypeEnum.App
                    }
                };

            // Act
            var result = package.GenerateReferences(references, new HelperOptions { GenerateHeaderAndFooter = true });

            // Assert
            result.ShouldBe(@"<script src=""/hotglue.axd/Scripts/depper1.js-app""></script>
");
        }

        [Test]
        public void Should_Generate_HTML_References_with_pre_and_post_text()
        {
            // Arrange
            var compilers = new[] { new JQueryTemplateCompiler(), };
            var referencer = new LABjsScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new SystemReference(new DirectoryInfo("C:/Root/"), new FileInfo("C:/Root/Scripts/depper1.js"), "depper1.js")
                    {
                        Type = Reference.TypeEnum.App
                    }
                };

            // Act
            var result = package.GenerateReferences(references, new HelperOptions { GenerateHeaderAndFooter = true });

            // Assert
            result.ShouldBe(
@"<script>
$LAB
.script(""/hotglue.axd/Scripts/depper1.js-app"");
</script>
");
        
        }

        [Test]
        public void Should_Generate_HTML_References_without_pre_and_post_text()
        {
            // Arrange
            var compilers = new[] { new JQueryTemplateCompiler(), };
            var referencer = new LABjsScriptReference();

            var package = new Package(".", compilers, referencer);

            var references = new[]
                {
                    new SystemReference(new DirectoryInfo("C:/Root/"), new FileInfo("C:/Root/Scripts/depper1.js"), "depper1.js")
                    {
                        Type = Reference.TypeEnum.App
                    }
                };

            // Act
            var result = package.GenerateReferences(references, new HelperOptions { GenerateHeaderAndFooter = false });

            // Assert
            result.ShouldBe(".script(\"/hotglue.axd/Scripts/depper1.js-app\")");
        }
    }
}