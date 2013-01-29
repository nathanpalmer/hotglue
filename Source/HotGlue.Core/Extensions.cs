using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    public static class Extensions
    {
        private static readonly Regex FileNameRegex = new Regex(@"(?<file>\S+)(?<extension>\.(.+(?=(-module|-glue|-require|-app))|.+))");

        public static string Reslash(this string input)
        {
            return input.Replace("\\", "/");
        }

        public static Reference.TypeEnum GetTypeEnum(this string typeString, Reference.TypeEnum defaultType)
        {
            if (String.Equals("library", typeString, StringComparison.OrdinalIgnoreCase))
            {
                return Reference.TypeEnum.Library;
            }
            return defaultType;
        }

        public static string RealFileName(this string fileName)
        {
            var match = FileNameRegex.Match(fileName);
            var name = match.Groups["file"].Value + match.Groups["extension"].Value;
            return name;
        }
    }
}
