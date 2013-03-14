using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Compilers;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Assembly.Compressor.Tests
{
    [TestFixture]
    public class TestAssemblyScanning
    {
        [Test]
        public void Does_It_Find_YUI_in_Release_mode()
        {
            // Arrange/Act
            var configuration = HotGlueConfiguration.Load(false);

            // Assert
            configuration.Debug.ShouldBe(false);
            configuration.ScriptPath.ShouldBe(@"Scripts/");
            configuration.Compilers.Length.ShouldBe(2);
            configuration.Compilers.Any(x => x.Type == typeof(YUICompressor).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Length.ShouldBe(4);
            configuration.Referencers.Any(x => x.Type == typeof(SlashSlashEqualReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(TripleSlashReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(RequireReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.GenerateScript.ShouldBe(null);
        }

        [Test]
        public void Does_it_add_yui_at_the_end()
        {
            // Arrange/Act
            var configuration = HotGlueConfiguration.Load(false);

            // Assert
            configuration.Debug.ShouldBe(false);
            configuration.ScriptPath.ShouldBe(@"Scripts/");
            configuration.Compilers.Length.ShouldBe(2);
            configuration.Compilers.First().Type.ShouldBe(typeof (CoffeeScriptCompiler).AssemblyQualifiedName);
            configuration.Compilers.Last().Type.ShouldBe(typeof(YUICompressor).AssemblyQualifiedName);
            configuration.Referencers.Length.ShouldBe(4);
            configuration.Referencers.Any(x => x.Type == typeof(SlashSlashEqualReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(TripleSlashReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(RequireReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.GenerateScript.ShouldBe(null);
        }


        [Test]
        public void Does_It_Skip_YUI_in_Debug_mode()
        {
            // Arrange/Act
            var configuration = HotGlueConfiguration.Load(true);

            // Assert
            configuration.Debug.ShouldBe(true);
            configuration.ScriptPath.ShouldBe(@"Scripts/");
            configuration.Compilers.Length.ShouldBe(2);
            configuration.Compilers.Any(x => x.Type == typeof(YUICompressor).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Length.ShouldBe(4);
            configuration.Referencers.Any(x => x.Type == typeof(SlashSlashEqualReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(TripleSlashReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(RequireReference).AssemblyQualifiedName).ShouldBe(true);
            configuration.GenerateScript.ShouldBe(null);
        }
    }
}
