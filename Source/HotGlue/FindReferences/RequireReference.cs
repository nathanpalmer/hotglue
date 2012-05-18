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
            @"^\s*(?:var\s+)?(?<variable>\S+)\s*=\s*require\((""|')?(?<path>.+?)(""|')?\)\s*;?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<Reference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceVariableRegex.Matches(fileText)
                .Cast<Match>()
                .Select(m => new Reference() {Name = m.Groups["path"].Value, Type = Reference.TypeEnum.Module})
                .Where(m => !String.IsNullOrWhiteSpace(m.Name));

            foreach (var match in matches)
            {
                yield return match;
            }
        }
    }
}