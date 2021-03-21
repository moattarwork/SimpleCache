using System;

namespace SimpleCache
{
    public sealed class CacheOptions<TKey, TValue>
    {
        public Action<CacheNode<TKey, TValue>> OnItemEvicted { get; set; }
    }
}