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
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
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
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Parse_TripleSlashReference()
        {
            // Arrange
            var referencer = new TripleSlashReference();

            // Act
            var references = referencer.Parse(@"
/// <reference path=""test.js""/>
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("test.js");
        }

        [Test]
        public void Should_Parse_TripleSlashWithSpaces()
        {
            // Arrange
            var referencer = new TripleSlashReference();

            // Act
            var references = referencer.Parse(@"
///      <reference      path=""test.js""      />       
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("test.js");
        }

        [Test]
        public void Should_Find_CoffeeScript_Module()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse("mod4 = require('mod4.coffee')\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("mod4.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }
    }
}
