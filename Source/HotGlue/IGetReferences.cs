using System;
using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IGetReferences
    {
        Dictionary<string, IList<FileReference>> Parse(HotGlueConfiguration config, String rootPath, String relativePath, String fileName);
    }
}