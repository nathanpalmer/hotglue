using System;
using System.Collections.Generic;

namespace HotGlue
{
    public interface IFindReference
    {
        IEnumerable<FileReference> Parse(String fileText);
    }
}