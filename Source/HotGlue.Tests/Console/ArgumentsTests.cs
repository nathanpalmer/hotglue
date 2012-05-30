using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using HotGlue.Console;

namespace HotGlue.Tests.Console
{
    [TestFixture]
    public class ArgumentsTests
    {
        [Test]
        public void Parse_should_error_for_no_arguments()
        {
            var input = Enumerable.Empty<string>();

            var actual = Arguments.Parse(input);

            actual.Error.ShouldBe(HotGlue.Console.Properties.Resources.UsageHelp);
        }

        [Test]
        public void Parse_should_handle_one_filename()
        {
            var infile = "foo.js";
            var input = new [] { infile };

            var actual = Arguments.Parse(input);

            actual.InFilenames.ShouldNotBeEmpty();
            actual.InFilenames.First().ShouldBe(infile);
            actual.Error.ShouldBe(null);
        }

        [Test]
        public void Parse_should_handle_outfile()
        {
            var infile = "foo.js";
            var outfile = "bar.js";
            var input = new[] { infile, "-o", outfile };

            var actual = Arguments.Parse(input);

            actual.InFilenames.ShouldNotBeEmpty();
            actual.InFilenames.First().ShouldBe(infile);
            actual.OutFilename.ShouldBe(outfile);
            actual.Error.ShouldBe(null);
        }

        [Test]
        public void Parse_should_handle_multiple_infilenames()
        {
            var infile = "foo.js";
            var infile2 = "baz.js";
            var outfile = "bar.js";
            var input = new[] { infile, infile2, "-o", outfile };

            var actual = Arguments.Parse(input);

            actual.InFilenames.ShouldNotBeEmpty();
            actual.InFilenames.ShouldContain(infile);
            actual.InFilenames.ShouldContain(infile2);
            actual.OutFilename.ShouldBe(outfile);
            actual.Error.ShouldBe(null);
        }

        [Test]
        public void Parse_should_error_on_extra_params()
        {
            var infile = "foo.js";
            var outfile = "bar.js";
            var crap = "misplaced";
            var input = new[] { infile, "-o", outfile, crap };

            var actual = Arguments.Parse(input);

            actual.Error.ShouldBe(HotGlue.Console.Properties.Resources.UsageHelp);
        }

        [Test]
        public void Parse_should_error_on_unrecognized_switch()
        {
            var input = new[] { "-?" };

            var actual = Arguments.Parse(input);

            actual.Error.ShouldBe(HotGlue.Console.Properties.Resources.UsageHelp);
        }
    }
}
