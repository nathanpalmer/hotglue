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
                Referencers = new HotGlueReference[]
                    {
                        new HotGlueReference { Type = typeof(SlashSlashEqualReference).FullName }, 
                        new HotGlueReference { Type = typeof(RequireReference).FullName },
                        new HotGlueReference { Type = typeof(TripleSlashReference).FullName }
                    }
            };

        [Test]
        public void Parse_And_Retrun_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "graph_test.js", Path = configuration.ScriptPath + "Module8"};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() { Name = "dep1.js", Path = configuration.ScriptPath + "Module8" }).ShouldBe(true);
            references.Contains(new Reference() { Name = "module1.js", Path = configuration.ScriptPath + "Module8" }).ShouldBe(true);
            references.Contains(new Reference() { Name = "graph_test.js", Path = configuration.ScriptPath + "Module8" }).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() { Name = "dep1.js", Path = configuration.ScriptPath + "Module8" }).ShouldBe(true);
            references[1].Equals(new Reference() { Name = "module1.js", Path = configuration.ScriptPath + "Module8" }).ShouldBe(true);
            references[2].Equals(new Reference() { Name = "graph_test.js", Path = configuration.ScriptPath + "Module8" }).ShouldBe(true);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Depedencies_At_Multiple_Levels_Should_Not_Be_Circular(bool specifyRoot)
        {
            // Arrange
            if (specifyRoot)
            {
                configuration.ScriptPath = "Scripts/";
            }
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module1"};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            // check in list
            references.Contains(new Reference() { Name = "dep1.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            references.Contains(new Reference() {Name = "mod.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            references.Contains(new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() {Name = "dep1.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            references[1].Equals(new Reference() {Name = "mod.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
            references[2].Equals(new Reference() {Name = "app.js", Path = configuration.ScriptPath + "Module1"}).ShouldBe(true);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Circular reference detected", MatchType = MessageMatch.Contains)]
        public void Detect_Circular_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() {Name = "circular_begin.js", Path = configuration.ScriptPath + "Exception1"};

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
            var reference = new Reference() {Name = "reference.js", Path = configuration.ScriptPath + "Exception2"};

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
            var reference = new Reference() {Name = "reference_forever.js", Path = configuration.ScriptPath + "Exception3"};

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
            references[0].Path.ShouldBe(configuration.ScriptPath + "Module2");
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
            references.Contains(new Reference() { Name = "ext1.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);
            references.Contains(new Reference() { Name = "ext2.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);
            // check order
            references[0].Equals(new Reference() { Name = "ext2.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);
            references[1].Equals(new Reference() { Name = "ext1.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);
            references[2].Equals(new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module3" }).ShouldBe(true);

        }

        [Test]
        public void Can_Parse_Relative_Paths_Within_References()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference {Name = "app.js", Path = configuration.ScriptPath + "Module4"};

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            references.Contains(new Reference {Name = "app.js", Path = configuration.ScriptPath + "Module4"}).ShouldBe(true);
            references.Contains(new Reference {Name = "mod.js", Path = configuration.ScriptPath + "Module4-Relative"}).ShouldBe(true);
            references.Contains(new Reference {Name = "dep1.js", Path = configuration.ScriptPath + "Module4-Relative"}).ShouldBe(true);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Can_Parse_Relative_Paths_And_Their_Modules(bool specifyRoot)
        {
            // Arrange
            if (specifyRoot)
            {
                configuration.ScriptPath = "Scripts/";
            }
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference { Name = "app.js", Path = configuration.ScriptPath + "Module6" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            references.Contains(new Reference { Name = "app.js", Path = configuration.ScriptPath + "Module6" }).ShouldBe(true);
            references.Contains(new Reference { Name = "mod1.js", Path = configuration.ScriptPath + "Module5" }).ShouldBe(true);
            references.Contains(new Reference { Name = "mod2.js", Path = configuration.ScriptPath + "Module5" }).ShouldBe(true);
        }

        [Test]
        public void Should_Not_Parse_Library_References()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference { Name = "app.js", Path = configuration.ScriptPath + "LibraryTest1" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references.Contains(new Reference { Name = "app.js", Path = configuration.ScriptPath + "LibraryTest1" }).ShouldBe(true);
            references.Contains(new Reference { Name = "library.js", Path = configuration.ScriptPath + "LibraryTest1" }).ShouldBe(true);
        }

        [Test]
        public void More_Than_One_Library_Only_Adds_Once()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference { Name = "app.js", Path = configuration.ScriptPath + "LibraryTest2" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(3);
            references.Contains(new Reference { Name = "app.js", Path = configuration.ScriptPath + "LibraryTest2" }).ShouldBe(true);
            references.Contains(new Reference { Name = "app2.js", Path = configuration.ScriptPath + "LibraryTest2" }).ShouldBe(true);
            references.Contains(new Reference { Name = "library.js", Path = configuration.ScriptPath + "LibraryTest2" }).ShouldBe(true);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "A different require reference was found for the file", MatchType = MessageMatch.Contains)]
        public void Detect_Multiple_Different_Require_Types_For_Same_Reference_In_Same_File()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() { Name = "reference.js", Path = configuration.ScriptPath + "Exception4"};

            var references = locator.Load(root, reference);
            foreach (var reference1 in references)
            {
                // Assert
                Assert.Fail("Expected different require type type exception");
            }
        }

        [Test]
        public void Ignore_Multiple_Same_Require_Types_For_Same_Reference_In_Same_File()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module9" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references.Contains(new Reference { Name = "app.js", Path = configuration.ScriptPath + "Module9" }).ShouldBe(true);
            references.Contains(new Reference { Name = "module.js", Path = configuration.ScriptPath + "Module9" }).ShouldBe(true);
        }

        [Test]
        public void Multiple_Files_With_Same_Name_Different_Locations_Get_Added()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = new Reference() { Name = "app.js", Path = configuration.ScriptPath + "Module10" };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(4);
            references.Contains(new Reference { Name = "app.js", Path = configuration.ScriptPath + "Module10" }).ShouldBe(true);
            references.Contains(new Reference { Name = "dep1.js", Path = configuration.ScriptPath + "Module10/sub1" }).ShouldBe(true);
            references.Contains(new Reference { Name = "dep1.js", Path = configuration.ScriptPath + "Module10/sub2" }).ShouldBe(true);
            references.Contains(new Reference { Name = "dep1.js", Path = configuration.ScriptPath + "Module10/sub3" }).ShouldBe(true);
        }
    }
}
