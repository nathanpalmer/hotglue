using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Compilers;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Assembly.PartialConfiguration.Tests
{
    [TestFixture]
    public class TestAssemblyScanning
    {
        [Test]
        public void Does_it_find_assemblies_when_a_configuration_exists()
        {
            // Arrange/Act
            var configuration = HotGlueConfiguration.Load(false);

            // Assert
            configuration.Debug.ShouldBe(false);
            configuration.ScriptPath.ShouldBe(@"js/");
            configuration.Compilers.Length.ShouldBe(1);
            configuration.Compilers.Any(x => x.Type == typeof(TypeScriptCompiler).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Length.ShouldBe(4);
            configuration.Referencers.Any(x => x.Type == typeof(SlashSlashEqualReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(TripleSlashReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(RequireReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.GenerateScript.ShouldBe(null);
        }
    }
}
