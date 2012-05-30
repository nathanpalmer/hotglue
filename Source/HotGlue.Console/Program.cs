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
                PromptIfDebugging();
                return -1;
            }
            var result = Run(arguments);
            PromptIfDebugging();
            return result;
        }

        static int Run(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            var root = Path.GetDirectoryName(Path.GetFullPath(arguments.InFilename));
            var filename = Path.GetFileName(arguments.InFilename);
            var allScripts = Concatenator.Compile(filename, root);
            File.WriteAllText(OutputFileName(arguments), allScripts, Encoding.UTF8);
            return 0;
        }

        private static void PromptIfDebugging()
        {
            if (Debugger.IsAttached)
            {
                System.Console.WriteLine("All done; press Enter to quit.");
                System.Console.ReadLine();
            }
        }

        internal static string OutputFileName(Arguments arguments)
        {
            if (!String.IsNullOrEmpty(arguments.OutFilename))
            {
                return arguments.OutFilename;
            }
            if (string.IsNullOrEmpty(arguments.InFilename))
            {
                throw new ArgumentException("You must supply either OutFilename or at least one InFilename");
            }
            return Path.ChangeExtension(arguments.InFilename, ".all.js");
        }
    }
}
