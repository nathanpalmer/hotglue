using System;
using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IFindReference
    {
        IEnumerable<Reference> Parse(String fileText);
    }
}