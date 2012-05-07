using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HotGlue
{
    public class RequireReference : IFindReference
    {
        private static readonly Regex ReferenceVariableRegex = new Regex(
            @"^\s*var\s+(?<variable>\S+)\s*=\s*require\((""|')?(?<path>.+?)(""|')?\)\s*;?",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<FileReference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceVariableRegex.Matches(fileText)
                .Cast<Match>()
                .Select(m => new FileReference() {Name = m.Groups["path"].Value, Variable = m.Groups["variable"].Value, Wrap = true})
                .Where(m => !String.IsNullOrWhiteSpace(m.Name) && !String.IsNullOrWhiteSpace(m.Variable));

            foreach (var match in matches)
            {
                yield return match;
            }
        }
    }
}