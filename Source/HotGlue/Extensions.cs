using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public static class Extensions
    {
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
    }
}
