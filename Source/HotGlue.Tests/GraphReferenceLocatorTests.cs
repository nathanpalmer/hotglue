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
        private HotGlueConfiguration configuration = new HotGlueConfiguration()
                                                         {
                                                             ScriptPath = "..\\..\\Scripts"
                                                         };

        [Test]
        public void Parse_And_Retrun_Reference()
        {
            // Arrange
            var referencers = new List<IFindReference>() { new SlashSlashEqualReference() };
            var locator = new GraphReferenceLocator(configuration, referencers);
            var reference = new Reference() { Name = "graph_test.js", Path = configuration.ScriptPath };

            var references = locator.Load(configuration.ScriptPath, reference).ToList();
            Assert.AreEqual(3, references.Count);
            // check in list
            Assert.True(references.Contains(new Reference() { Name = "dep1.js", Path = configuration.ScriptPath }));
            Assert.True(references.Contains(new Reference() { Name = "module1.js", Path = configuration.ScriptPath }));
            Assert.True(references.Contains(new Reference() { Name = "graph_test.js", Path = configuration.ScriptPath }));
            // check order
            Assert.True(references[0].Equals(new Reference() { Name = "dep1.js", Path = configuration.ScriptPath }));
            Assert.True(references[1].Equals(new Reference() { Name = "module1.js", Path = configuration.ScriptPath }));
            Assert.True(references[2].Equals(new Reference() { Name = "graph_test.js", Path = configuration.ScriptPath }));
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Circular reference detected", MatchType = MessageMatch.Contains)]
        public void Detect_Circular_Reference()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();
            var locator = new GraphReferenceLocator(configuration, new[] { referencer });
            var reference = new Reference() {Name = "circular_begin.js", Path = configuration.ScriptPath};

            var references = locator.Load(configuration.ScriptPath, reference);
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

            var references = locator.Load(configuration.ScriptPath, reference);
            foreach (var reference1 in references)
            {
                // Assert
                Assert.Fail("Expected different require type type exception");    
            }
        }
    }
}
