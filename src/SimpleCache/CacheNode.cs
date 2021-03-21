using System;

namespace SimpleCache
{
    public sealed class CacheNode<TKey, TValue>
    {
        public CacheNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            LastAccess = DateTimeOffset.Now;
        }
        
        public TKey Key { get; }
        public TValue Value { get; set; }
        public DateTimeOffset LastAccess { get; set; }
    }
}