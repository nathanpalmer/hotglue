using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotGlue.Compilers;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Assembly.CoffeeScript.Tests
{
    [TestFixture]
    public class TestAssemblyScanning
    {
        [Test]
        public void Does_It_Find_Only_The_Basic_Classes()
        {
            // Arrange/Act
            var configuration = HotGlueConfiguration.Load();

            // Assert
            configuration.Debug.ShouldBe(false);
            configuration.ScriptPath.ShouldBe(@"Scripts/");
            configuration.Compilers.Length.ShouldBe(1);
            configuration.Compilers.Any(x => x.Type == typeof(CoffeeScriptCompiler).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Length.ShouldBe(3);
            configuration.Referencers.Any(x => x.Type == typeof(SlashSlashEqualReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(TripleSlashReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(RequireReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.GenerateScript.ShouldBe(null);
        }
    }
}
