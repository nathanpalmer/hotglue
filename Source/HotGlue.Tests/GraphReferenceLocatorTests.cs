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
                ScriptSharedPath = "Scripts\\Shared\\",
                Referencers = new HotGlueReference[]
                    {
                        new HotGlueReference { Type = typeof(SlashSlashEqualReference).FullName }, 
                        new HotGlueReference { Type = typeof(RequireReference).FullName }
                    }
            };

        [Test]
        public void Parse_And_Retrun_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "graph_test.js", Path = configuration.ScriptPath};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() {Name = "dep1.js", Path = configuration.ScriptPath}).ShouldBe(true);
            references.Contains(new Reference() {Name = "module1.js", Path = configuration.ScriptPath}).ShouldBe(true);
            references.Contains(new Reference() {Name = "graph_test.js", Path = configuration.ScriptPath}).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() {Name = "dep1.js", Path = configuration.ScriptPath}).ShouldBe(true);
            references[1].Equals(new Reference() {Name = "module1.js", Path = configuration.ScriptPath}).ShouldBe(true);
            references[2].Equals(new Reference() {Name = "graph_test.js", Path = configuration.ScriptPath}).ShouldBe(true);
        }

        [Test]
        public void Depedencies_At_Multiple_Levels_Should_Not_Be_Circular()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module1"};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() {Name = "dep1.js", Path = configuration.ScriptSharedPath}).ShouldBe(true);
            references.Contains(new Reference() {Name = "mod.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            references.Contains(new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() {Name = "dep1.js", Path = configuration.ScriptSharedPath}).ShouldBe(true);
            references[1].Equals(new Reference() {Name = "mod.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            references[2].Equals(new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Circular reference detected", MatchType = MessageMatch.Contains)]
        public void Detect_Circular_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
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
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "reference.js", Path = configuration.ScriptPath};

            var references = locator.Load(root, reference);
            foreach (var reference1 in references)
            {
                // Assert
                Assert.Fail("Expected different require type type exception");
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Circular reference detected", MatchType = MessageMatch.Contains)]
        public void You_Cannot_Reference_Yourself()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "reference_forever.js", Path = configuration.ScriptPath};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            Assert.Fail("Expected circular reference detected exception");
        }

        [Test]
        public void Reference_Path_Should_Be_Unmodified()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module2"};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references[0].Path.ShouldBe(configuration.ScriptSharedPath);
            references[1].Path.ShouldBe(configuration.ScriptPath + "Module2");
        }

        [Test]
        public void Reference_Type_Should_Be_Unmodified()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module2" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references[1].Type.ShouldBe(Reference.TypeEnum.App);
        }

        [Test]
        public void Reference_Type_Dependency_Should_Be_Dependency()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module2" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references[0].Type.ShouldBe(Reference.TypeEnum.Dependency);
        }

        [Test]
        public void Order_Of_External_Dependencies_Should_Stay_The_Same_When_Taken_From_Shared()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module3" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);
            references.Contains(new Reference() { Name = "ext1.js", Path = configuration.ScriptSharedPath }).ShouldBe(true);
            references.Contains(new Reference() { Name = "ext2.js", Path = configuration.ScriptSharedPath }).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() { Name = "ext2.js", Path = configuration.ScriptSharedPath }).ShouldBe(true);
            references[1].Equals(new Reference() { Name = "ext1.js", Path = configuration.ScriptSharedPath }).ShouldBe(true);
            references[2].Equals(new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);

        }
    }
}
