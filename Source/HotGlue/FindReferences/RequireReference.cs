using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    public class RequireReference : IFindReference
    {
        private static readonly Regex ReferenceVariableRegex = new Regex(
            @"^\s*(?:var\s+)?(?<variable>\S+)\s*=\s*require\((""|')?(?<path>.+?)(""|')?\)\S*;?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<RelativeReference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceVariableRegex.Matches(fileText)
                .Cast<Match>()
                .Where(m => !String.IsNullOrWhiteSpace(m.Groups["path"].Value))
                .Select(m => new RelativeReference(m.Groups["path"].Value) { Type = Reference.TypeEnum.Module });

            foreach (var match in matches)
            {
                yield return match;
            }
        }
    }
}