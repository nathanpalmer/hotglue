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
                .Where(m => !String.IsNullOrWhiteSpace(m.Groups["path"].Value))
                .Select(m => new Reference(m.Groups["path"].Value) { Type = m.Groups["identifier"].Value.GetTypeEnum(Reference.TypeEnum.Dependency)});

            foreach(var match in matches)
            {
                yield return match;
            }
        }
    }
}