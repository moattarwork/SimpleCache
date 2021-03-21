# SimpleCache

SimpleCache is a simple solution for caching. It's a library written in .net 5.0.

## How to use

The Unit test project explains different type of the usage for the Cache. Here are some samples from the usages:

```csharp

// Simple use 
var cache = new Cache<string>();

cache.Set("key 1", "Sample values");
cache.Set("key 2", "Sample other values");

var cachedValue = cache.Get("key 1");


// Cache with notification 
var cache = new Cache<string>(2, opt => { opt.OnItemEvicted = n => /* Handling of the notification*/ });


```