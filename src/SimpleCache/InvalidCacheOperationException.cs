using System;

namespace SimpleCache
{
    public class InvalidCacheOperationException : Exception
    {
        public InvalidCacheOperationException(string message) : base(message) {}
    }
}