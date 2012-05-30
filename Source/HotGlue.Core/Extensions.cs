using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue
{
    public static class Extensions
    {
        public static string Reslash(this string input)
        {
            return input.Replace("\\", "/");
        }
    }
}
