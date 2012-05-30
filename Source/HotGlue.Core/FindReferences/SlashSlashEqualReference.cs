using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    public class SlashSlashEqualReference : IFindReference
    {
        static readonly Regex ReferenceCommentRegex = new Regex(
            @"^\s*(//|\*|#)=\s*(?<identifier>require|library)\s*(""|')?(?<path>.+?)(""|')?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<Reference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceCommentRegex.Matches(fileText)
                .Cast<Match>()
                .Select(m => new Reference() {Name = m.Groups["path"].Value, Type = m.Groups["identifier"].Value == "library" ? Reference.TypeEnum.Library : Reference.TypeEnum.Dependency})
                .Where(m => !String.IsNullOrWhiteSpace(m.Name));

            foreach(var match in matches)
            {
                yield return match;
            }
        }
    }
}