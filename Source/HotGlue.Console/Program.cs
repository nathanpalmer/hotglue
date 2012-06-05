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
            try
            {
                var infileWithPath = Path.GetFullPath(arguments.InFilename);
                var fullPath = Path.GetDirectoryName(infileWithPath);
                var scriptPath = FindScriptFolderPath(infileWithPath, arguments.ScriptPath);
                if (string.IsNullOrEmpty(scriptPath))
                {
                    System.Console.Error.WriteLine("Could not find script folder '{0}' in path '{1}'.", 
                        arguments.ScriptPath,  // 0
                        infileWithPath         // 1
                        );
                    return -1;
                }
                var filename = Path.GetFileName(infileWithPath);
                var allScripts = Concatenator.Compile(scriptPath, fullPath, filename);
                File.WriteAllText(OutputFileName(arguments), allScripts, Encoding.UTF8);
                return 0;
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                if (Debugger.IsAttached)
                {
                    System.Console.Error.WriteLine(ex.StackTrace);
                }
                return -1;
            }
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

        internal static String FindScriptFolderPath(String fullFilePath, String scriptFolderName)
        {
            if (string.IsNullOrEmpty(fullFilePath))
            {
                throw new ArgumentNullException("fullFilePath");
            }
            if (string.IsNullOrEmpty(scriptFolderName))
            {
                throw new ArgumentNullException("scriptFolderName");
            }
            String current = fullFilePath;
            while (!string.IsNullOrEmpty(current))
            {
                var dirName = Path.GetDirectoryName(current);
                var fileName = Path.GetFileName(dirName);
                if (string.Equals(scriptFolderName, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(dirName);
                }
                current = dirName;
            }
            return null;
        }
    }
}
