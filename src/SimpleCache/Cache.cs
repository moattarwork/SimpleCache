using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleCache
{
    public sealed class Cache<TValue>
    {
        public const int DefaultCacheCapacity = 100;

        private readonly LinkedList<CacheNode<string, TValue>> _frequency = new();
        private readonly ConcurrentDictionary<string, CacheNode<string, TValue>> _lookup = new();

        private readonly CacheOptions<string, TValue> _options = new();

        public Cache(int capacity = DefaultCacheCapacity, Action<CacheOptions<string, TValue>> configure = null)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
            configure?.Invoke(_options);
        }

        public int Capacity { get; }
        public int Count => _lookup.Count;

        public TValue Get(string key)
        {
            lock (_frequency)
            {
                var result = _lookup.TryGetValue(key, out var node);
                if (!result)
                    return default;

                MarkAsMostRecentlyUsed(node);
                return node.Value;
            }
        }

        public void Set(string key, TValue value)
        {
            lock (_frequency)
            {
                var result = _lookup.TryGetValue(key, out var node);
                if (result)
                {
                    lock (node)
                    {
                        node.Value = value;
                    }

                    MarkAsMostRecentlyUsed(node);
                }
                else
                {
                    node = new CacheNode<string, TValue>(key, value);
                    lock (_frequency)
                    {
                        if (Count == Capacity)
                            RemoveLeastRecentlyUsed();

                        AddAsMostRecentlyUsed(node);

                        if (!_lookup.TryAdd(key, node))
                            throw new InvalidCacheOperationException(
                                $"Error in adding new node with key:{key} to the cache");
                    }
                }
            }
        }

        private void MarkAsMostRecentlyUsed(CacheNode<string, TValue> node)
        {
            AddAsMostRecentlyUsed(node, true);
        }

        private void AddAsMostRecentlyUsed(CacheNode<string, TValue> node, bool removeNode = false)
        {
            if (removeNode)
                _frequency.Remove(node);

            node.LastAccess = DateTimeOffset.Now;
            _frequency.AddFirst(node);
        }

        private void RemoveLeastRecentlyUsed()
        {
            var linkListNode = _frequency.Last;
            if (linkListNode == null)
                return;

            var node = linkListNode.Value;
            var nodeKey = node.Key;

            _frequency.RemoveLast();
            if (!_lookup.TryRemove(nodeKey, out var _))
                throw new InvalidCacheOperationException(
                    $"Error in removing node with key:{nodeKey} from the cache");

            _options.OnItemEvicted?.Invoke(node);
        }
    }
}