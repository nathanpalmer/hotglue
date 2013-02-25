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
    ///    /// <reference path="test.js"/>           OR
    ///    /// <reference path="test.js" library/>
    /// </summary>
    public class TripleSlashReference : IFindReference
    {
        static readonly Regex ReferenceCommentRegex = new Regex(
            @"^\s*///\s*<reference\s+path=""(?<path>.+?)""\s*(?<identifier>library?)?\s*/>\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<RelativeReference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceCommentRegex.Matches(fileText)
                .Cast<Match>()
                .Where(m => !String.IsNullOrWhiteSpace(m.Groups["path"].Value))
                .Select(MatchToRelativeReference);

            foreach (var match in matches)
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