using System.Collections.Concurrent;

namespace HotGlue
{
    public class DictionaryCache : IFileCache
    {
        private ConcurrentDictionary<string, object> _cache;

        public DictionaryCache()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }

        public dynamic Get(string fullName)
        {
            object value;
            return _cache.TryGetValue(fullName, out value) ? value : null;
        }

        public void Set(string fullName, dynamic o)
        {
            _cache.TryAdd(fullName, o);
        }
    }
}