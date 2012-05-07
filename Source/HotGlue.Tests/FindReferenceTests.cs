using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class FindReferenceTests
    {
        [Test]
        public void Can_Parse_Comment_Reference()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();

            // Act
            var references = referencer.Parse(@"
//= require module1.js
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
        }

        [Test]
        public void Can_Parse_Module_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod = require('module1.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
        }

        [Test]
        public void Can_Parse_Multiple_Module_References()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = require('module1.js');
var mod2 = require('module2.js');

");
            // Assert
            references.Count().ShouldBe(2);
            references.First().Name.ShouldBe("module1.js");
            references.First().Wrap.ShouldBe(true);
        }

        [Test]
        public void Should_Not_Parse_Commented_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = require('module1.js');
//var mod2 = require('module2.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Wrap.ShouldBe(true);
        }
    }
}
