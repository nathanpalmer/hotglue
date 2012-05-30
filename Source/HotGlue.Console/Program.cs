using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HotGlue.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            var arguments = Arguments.Parse(args);
            if (!string.IsNullOrEmpty(arguments.Error))
            {
                System.Console.Error.WriteLine(arguments.Error);
                return -1;
            }
            var result = Run(arguments);
            if (Debugger.IsAttached)
            {
                System.Console.ReadLine();
            }
            return result;
        }

        static int Run(Arguments arguments)
        {
            var allScripts = Concatenator.CompileAll(arguments.InFilenames);
            File.WriteAllText(OutputFileName(arguments), allScripts, Encoding.UTF8);
            return 0;
        }

        internal static string OutputFileName(Arguments arguments)
        {
            if (!String.IsNullOrEmpty(arguments.OutFilename))
            {
                return arguments.OutFilename;
            }
            var firstInFile = arguments.InFilenames.FirstOrDefault();
            if (string.IsNullOrEmpty(firstInFile))
            {
                throw new ArgumentException("You must supply either OutFilename or at least one InFilename");
            }
            return Path.ChangeExtension(firstInFile, ".all.js");
        }
    }
}
