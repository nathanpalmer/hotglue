using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HotGlue
{
    public class SlashSlashEqualReference : IFindReference
    {
        static readonly Regex ReferenceCommentRegex = new Regex(
            @"^\s*(//|\*|#)=\s*require\s*(""|')?(?<path>.+?)(""|')?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<FileReference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceCommentRegex.Matches(fileText)
                .Cast<Match>()
                .Select(m => new FileReference() {Name = m.Groups["path"].Value})
                .Where(m => !String.IsNullOrWhiteSpace(m.Name));

            foreach(var match in matches)
            {
                yield return match;
            }
        }
    }
}