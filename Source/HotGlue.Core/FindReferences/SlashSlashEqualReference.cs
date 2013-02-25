using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    /// <summary>
    /// Finds references in the format of
    /// 
    ///    //= require('test.js')   OR
    ///    #= require 'test.js'     OR
    ///    #= library 'test.js'     OR
    /// </summary>
    public class SlashSlashEqualReference : IFindReference
    {
        static readonly Regex ReferenceCommentRegex = new Regex(
            @"^\s*(//|\*|#)=\s*(?<identifier>require|library)\s*(""|')?(?<path>.+?)(""|')?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<RelativeReference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceCommentRegex.Matches(fileText)
                .Cast<Match>()
                .Where(m => !String.IsNullOrWhiteSpace(m.Groups["path"].Value))
                .Select(MatchToRelativeReference);

            foreach(var match in matches)
            {
                yield return match;
            }
        }

        private RelativeReference MatchToRelativeReference(Match match)
        {
            var pathGroup = match.Groups["path"];
            var identifierGroup = match.Groups["identifier"];
            return new RelativeReference(pathGroup.Value, pathGroup.Index) { Type = identifierGroup.Value.GetTypeEnum(Reference.TypeEnum.Dependency) };
        }
    }
}