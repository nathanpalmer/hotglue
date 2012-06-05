using System;
using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IFindReference
    {
        IEnumerable<RelativeReference> Parse(String fileText);
    }
}