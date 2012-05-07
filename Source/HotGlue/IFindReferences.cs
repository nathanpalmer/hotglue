using System;
using System.Collections.Generic;

namespace HotGlue
{
    public interface IFindReferences
    {
        IList<FileReference> Parse(String fileText);
    }
}