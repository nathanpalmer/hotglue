using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class GraphReferenceLocatorTests : TestBase
    {
        private string root = "..\\..\\";

        public GraphReferenceLocatorTests()
        {
            configuration = LoadedConfiguration.Load(new HotGlueConfiguration()
            {
                ScriptPath = "Scripts\\",
                Referencers = new HotGlueReference[]
                    {
                        new HotGlueReference { Type = typeof(SlashSlashEqualReference).FullName }, 
                        new HotGlueReference { Type = typeof(RequireReference).FullName },
                        new HotGlueReference { Type = typeof(TripleSlashReference).FullName }
                    }
            });
        }

        [Test]
        public void Parse_And_Return_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module8", "graph_test.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module8", "dep1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module8", "module1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module8", "graph_test.js", Reference.TypeEnum.App)
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
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
            var reference = BuildReference("Module1", "app.js", Reference.TypeEnum.App);
            var matchReferences = new []
                                  {
                                      BuildReference("Module1", "dep1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module1", "mod.js", Reference.TypeEnum.Module),
                                      BuildReference("Module1", "app.js", Reference.TypeEnum.App)
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Circular reference detected", MatchType = MessageMatch.Contains)]
        public void Detect_Circular_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Exception1", "circular_begin.js", Reference.TypeEnum.App);

            var references = locator.Load(root, reference).ToList();
            foreach (var r in references)
            {
                // Assert
                Assert.Fail("Expected circular reference detected exception");
            }
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException), ExpectedMessage = "Unable to find the file:", MatchType = MessageMatch.Contains)]
        public void Detect_Missing_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("ExceptionMissingFile", "app.js", Reference.TypeEnum.App);

            var references = locator.Load(root, reference);
            foreach (var r in references)
            {
                // Assert
                Assert.Fail("Expected file not found exception");
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "A different require reference was found for the file", MatchType = MessageMatch.Contains)]
        public void Detect_Multiple_Require_Types_For_Same_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Exception2", "reference.js", Reference.TypeEnum.App);

            var references = locator.Load(root, reference);
            foreach (var r in references)
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
            var reference = BuildReference("Exception3", "reference_forever.js", Reference.TypeEnum.App);

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
            var reference = BuildReference("Module2", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module2", "dep1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module2", "app.js", Reference.TypeEnum.App)
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Reference_Type_Should_Be_Unmodified()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module2", "app.js", Reference.TypeEnum.App);
            var matchReference = new[]
                                 {
                                     BuildReference("Module2", "dep1.js", Reference.TypeEnum.Dependency),
                                     reference
                                 };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReference, references);
        }

        [Test]
        public void Reference_Type_Dependency_Should_Be_Dependency()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module2", "app.js", Reference.TypeEnum.App);
            var matchReference = new[]
                                 {
                                     BuildReference("Module2", "dep1.js", Reference.TypeEnum.Dependency),
                                     reference
                                 };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReference, references);
        }

        [Test]
        public void Order_Of_External_Dependencies_Should_Stay_The_Same_When_Taken_From_Shared()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module3", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module3", "ext2.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module3", "ext1.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Can_Parse_Relative_Paths_Within_References()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module4", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module4-Relative", "dep1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module4-Relative", "mod.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Can_Find_Absolute_References()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("AbsoluteReference1", "app.js", Reference.TypeEnum.App);

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references.ShouldContain(r => r.Name.Equals("mod.js", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void Can_Find_Absolute_References_With_Tilde()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("AbsoluteReference2", "app.js", Reference.TypeEnum.App);

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            references.Count.ShouldBe(2);
            references.ShouldContain(r => r.Name.Equals("mod.js", StringComparison.InvariantCultureIgnoreCase));
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
            var reference = BuildReference("Module6", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module5", "mod2.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module5", "mod1.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Should_Add_Absolute_Reference()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module4", "app.js", Reference.TypeEnum.App);

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldContainExactlyOne(references, "/Scripts/Module4/app.js");
            ShouldContainExactlyOne(references, "/Scripts/Module4-Relative/dep1.js");
            ShouldContainExactlyOne(references, "/Scripts/Module4-Relative/mod.js");
        }

        [Test]
        public void Should_Not_Parse_Library_References()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("LibraryTest1", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("LibraryTest1", "library.js", Reference.TypeEnum.Library),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void More_Than_One_Library_Only_Adds_Once()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("LibraryTest2", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      //TODO: Original unit test didn't have this either but 
                                      //TODO  library.js requires something.js
                                      //BuildReference("LibraryTest2", "something.js", Reference.TypeEnum.Dependency),
                                      BuildReference("LibraryTest2", "library.js", Reference.TypeEnum.Library),
                                      BuildReference("LibraryTest2", "app2.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "A different require reference was found for the file", MatchType = MessageMatch.Contains)]
        public void Detect_Multiple_Different_Require_Types_For_Same_Reference_In_Same_File()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Exception4", "reference.js", Reference.TypeEnum.App);

            var references = locator.Load(root, reference);
            foreach (var r in references)
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
            var reference = BuildReference("Module9", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module9", "module.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };
            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Multiple_Files_With_Same_Name_Different_Locations_Get_Added()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Module10", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Module10/sub1", "dep1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module10/sub2", "dep1.js", Reference.TypeEnum.Dependency),
                                      BuildReference("Module10/sub3", "dep1.js", Reference.TypeEnum.Module),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Ignore_Multiple_References_In_Different_Relative_Paths()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Path1", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Path1\\Sub", "library.js", Reference.TypeEnum.Library),
                                      BuildReference("Path1\\Sub", "module.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }

        [Test]
        public void Duplicate_Relative_References_In_Same_File_Not_Found_Until_System_Duplicate_Check()
        {
            // Arrange
            var locator = new GraphReferenceLocator(configuration);
            var reference = BuildReference("Path2", "app.js", Reference.TypeEnum.App);
            var matchReferences = new[]
                                  {
                                      BuildReference("Path2", "module.js", Reference.TypeEnum.Dependency),
                                      reference
                                  };

            // Act
            var references = locator.Load(root, reference).ToList();

            // Assert
            ShouldMatch(matchReferences, references);
        }
    }

    public class TestBase
    {
        protected LoadedConfiguration configuration;

        public SystemReference BuildReference(string path, string name, Reference.TypeEnum type)
        {
            var root = new DirectoryInfo("../../");
            var reference = new SystemReference(root, new FileInfo(Path.Combine(root.FullName, configuration.ScriptPath + path + "/" + name)), name)
            {
                Type = type
            };
            return reference;
        }

        public void ShouldContainExactlyOne(IList<SystemReference> generated, string path)
        {
            var references =
                generated.Where(r => r.ReferenceNames.Contains(path, StringComparer.InvariantCultureIgnoreCase));
            references.Count().ShouldBe(1);
            references.Single().ReferenceNames.Count(rn => rn.Equals(path, StringComparison.InvariantCultureIgnoreCase)).ShouldBe(1);
        }

        public void ShouldMatch(Reference good, Reference generated)
        {
            generated.Name.ShouldBe(good.Name);
            generated.Path.ShouldBe(good.Path);
            generated.Type.ShouldBe(good.Type);
        }

        public void ShouldMatch(SystemReference[] good, List<SystemReference> generated)
        {
            generated.Count.ShouldBe(good.Length);
            
            // check in list
            for (int index = 0; index < good.Length; index++)
            {
                var reference = good[index];

                generated.Contains(reference).ShouldBe(true);
                ShouldMatch(reference, generated[index]);
            }
        }
    }
}
