using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class GraphReferenceLocatorTests
    {
        private string root = "..\\..\\";
        private HotGlueConfiguration configuration = new HotGlueConfiguration()
                                                         {
                                                             ScriptPath = "Scripts\\",
                                                             ScriptSharedFolder = "Scripts\\Shared\\"
                                                         };

        [Test]
        public void Parse_And_Retrun_Reference()
        {
            // Arrange
            var referencers = new List<IFindReference>() { new SlashSlashEqualReference() };
            var locator = new GraphReferenceLocator(configuration, referencers);
            var reference = new Reference() { Name = "graph_test.js", Path = configuration.ScriptPath };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() { Name = "dep1.js", Path = root + configuration.ScriptPath }).ShouldBe(true);
            references.Contains(new Reference() { Name = "module1.js", Path = root + configuration.ScriptPath }).ShouldBe(true);
            references.Contains(new Reference() { Name = "graph_test.js", Path = root + configuration.ScriptPath }).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() { Name = "dep1.js", Path = root + configuration.ScriptPath }).ShouldBe(true);
            references[1].Equals(new Reference() { Name = "module1.js", Path = root + configuration.ScriptPath }).ShouldBe(true);
            references[2].Equals(new Reference() { Name = "graph_test.js", Path = root + configuration.ScriptPath }).ShouldBe(true);
        }

        [Test]
        public void Depedencies_At_Multiple_Levels_Should_Not_Be_Circular()
        {
            // Arrange
            var referencers = new List<IFindReference>() { new SlashSlashEqualReference(), new RequireReference() };
            var locator = new GraphReferenceLocator(configuration, referencers);
            var reference = new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module1" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() { Name = "dep1.js", Path = root + configuration.ScriptSharedFolder }).ShouldBe(true);
            references.Contains(new Reference() { Name = "mod.js", Path = root + configuration.ScriptPath+"Module1" }).ShouldBe(true);
            references.Contains(new Reference() { Name = "app.js", Path = root + configuration.ScriptPath+"Module1" }).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() { Name = "dep1.js", Path = root + configuration.ScriptSharedFolder }).ShouldBe(true);
            references[1].Equals(new Reference() { Name = "mod.js", Path = root + configuration.ScriptPath+"Module1" }).ShouldBe(true);
            references[2].Equals(new Reference() { Name = "app.js", Path = root + configuration.ScriptPath + "Module1" }).ShouldBe(true);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Circular reference detected", MatchType = MessageMatch.Contains)]
        public void Detect_Circular_Reference()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();
            var locator = new GraphReferenceLocator(configuration, new[] { referencer });
            var reference = new Reference() {Name = "circular_begin.js", Path = configuration.ScriptPath};

            var references = locator.Load(root, reference).ToList();
            foreach (var reference1 in references)
            {
                // Assert
                Assert.Fail("Expected circular reference detected exception");    
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "A different require reference was found for the file", MatchType = MessageMatch.Contains)]
        public void Detect_Multiple_Require_Types_For_Same_Reference()
        {
            // Arrange
            var referencers = new List<IFindReference>() {new SlashSlashEqualReference(), new RequireReference()};
            var locator = new GraphReferenceLocator(configuration, referencers);
            var reference = new Reference() { Name = "reference.js", Path = configuration.ScriptPath };

            var references = locator.Load(root, reference);
            foreach (var reference1 in references)
            {
                // Assert
                Assert.Fail("Expected different require type type exception");    
            }
        }
    }
}
