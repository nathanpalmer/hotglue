using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using HotGlue.Console;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests.Console
{
    [TestFixture]
    public class ProgramTests
    {
        [Test]
        public void OutputFileName_should_return_OutFilename()
        {
            var input = new Arguments
                            {
                                OutFilename = "Expected.js"
                            };

            var actual = Program.OutputFileName(input);

            actual.ShouldBe(input.OutFilename);
        }

        [Test]
        public void OutputFileName_should_mangle_input_name_when_output_name_is_null()
        {
            var firstFilename = "foo.coffee";
            var input = new Arguments
            {
                InFilename = firstFilename,
                OutFilename = null
            };

            var actual = Program.OutputFileName(input);

            actual.ShouldBe("foo.all.js");
        }
    }
}
