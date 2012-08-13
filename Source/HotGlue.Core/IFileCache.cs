using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue
{
    public interface IFileCache
    {
        dynamic Get(string fullName);
        void Set(string fullName, dynamic o);
    }
}
