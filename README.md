# SynchronizedCache Example Project

This project demonstrates how to use the `SynchronizedCache` class to implement an in-memory cache that supports distributed invalidation.

## Azure Setup

Create a Service Bus resource, and then create a new Topic named 'synchronizedcache'. Copy the default primary connection string.

## Project Setup

Clone the repository. Enter you Service Bus connection string in Startup.cs.

Place a breakpoint in the `ProcessMessageAsync()` method of `SynchronizedCache.cs`.

## Testing the Cache

To increase the 'woah' factor, it is best to run the app on multiple computers at once. It will be fine if you don't, but it would be cool if you did.

This project contains an example of two caches - one that returns types of animals, and one that returns types of cars.
(It is admittedly very simple, contrived, and a bit pointless, but imagine that you are going to cache your super awesome objects instead if that helps).

Run the project and then browse to `/api/animal/dog` - this will load the Dog animal. Since it is the first run, it will not be cached and will be loaded from source (with an artificial 5 second delay to represent an expensive DB call or whatever).

Now refresh the page to load the Dog animal again - this time it will load from the cache and be instant.

Now browse to `/api/animal/cat` to load another item into the cache. Refresh the page to confirm that it loads immediately.

Now browse to `/api/animal/dog/flush` to force the Dog item to be removed from the cache. (Again, you can imagine a real business process that caused the item to change and need to be invalidated).

You should see your breakpoint get hit. When the item was invalidated from the cache, a message was sent to the service bus topic, which sent a copy to each subscriber. (If you had the app running on multiple machines, all of them should have been notified).

The `CarCache` is almost exactly the same, but demonstrates using an arbitrary object for the cache key. Cache retrievals and invalidations are done using a `CarType` object. Whenever the cache is accessed, the `GetHashKey()` function is called and lets you construct a composite key from the object.

You can use the following urls to test it out:

- `api/car/ford/focus`
- `api/car/tesla/modelX`
- `api/car/jeep/wrangler`

Then you can append `/flush` to any of them to remove them from the cache.
`