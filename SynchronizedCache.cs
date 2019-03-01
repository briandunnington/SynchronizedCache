using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizedCacheExample
{
    public interface ISynchronizedCache<TValue, TKey>
    {
        Task<TValue> GetAsync(TKey key);

        Task InvalidateAsync(TKey key);
    }

    public abstract class SynchronizedCache<TValue, TKey> : ISynchronizedCache<TValue, TKey>
    {
        private readonly ManagementClient serviceBusManagementClient;
        private readonly ITopicClient topicClient;
        private readonly string cacheType;
        private readonly string subscriptionName;

        private readonly Lazy<Task> lazyInitialization;
        private readonly ConcurrentDictionary<string, Task<TValue>> cache = new ConcurrentDictionary<string, Task<TValue>>();
        private ISubscriptionClient subscriptionClient;

        public SynchronizedCache(ManagementClient serviceBusManagementClient, ITopicClient serviceBusTopicClient)
        {
            this.serviceBusManagementClient = serviceBusManagementClient;
            this.topicClient = serviceBusTopicClient;

            this.cacheType = typeof(TValue).Name;
            this.subscriptionName = $"{cacheType}-{Environment.MachineName}";
            this.lazyInitialization = new Lazy<Task>(EnsureInitializationAsync, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Task<TValue> GetAsync(TKey key)
        {
            return GetAsync(key, GetHashKey(key));
        }

        public Task InvalidateAsync(TKey key)
        {
            return InvalidateAsync(key, GetHashKey(key));
        }


        protected abstract string GetHashKey(TKey key);

        protected abstract Task<TValue> Load(TKey key);

        protected async Task<TValue> GetAsync(TKey key, string hashKey)
        {
            await lazyInitialization.Value;

            return await cache.GetOrAdd(hashKey, (k) => Load(key));
        }

        protected async Task InvalidateAsync(TKey key, string hashKey)
        {
            await lazyInitialization.Value;

            var message = new Message();
            message.ContentType = cacheType;
            message.Label = hashKey;
            await topicClient.SendAsync(message);
        }


        async Task EnsureInitializationAsync()
        {
            if (!await serviceBusManagementClient.SubscriptionExistsAsync(topicClient.Path, subscriptionName))
            {
                var cacheTypeRule = new RuleDescription()
                {
                    Name = "CacheTypeRule",
                    Filter = new CorrelationFilter() { ContentType = cacheType }
                };
                var subscriptionDescription = new SubscriptionDescription(topicClient.Path, subscriptionName) { AutoDeleteOnIdle = TimeSpan.FromDays(1), UserMetadata = $"Created at: {DateTime.UtcNow}", DefaultMessageTimeToLive = TimeSpan.FromMinutes(15) };
                subscriptionDescription = await serviceBusManagementClient.CreateSubscriptionAsync(subscriptionDescription, cacheTypeRule);
            }
            this.subscriptionClient = new SubscriptionClient(topicClient.ServiceBusConnection, topicClient.Path, subscriptionName, ReceiveMode.PeekLock, RetryPolicy.Default);

            StartMessageListener();
        }

        void StartMessageListener()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            subscriptionClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);
        }

        async Task ProcessMessageAsync(Message message, CancellationToken token)
        {
            var lockToken = message.SystemProperties.LockToken;
            try
            {
                var key = (message.Label);
                cache.TryRemove(key, out _);
            }
            catch
            {
            }
            await subscriptionClient.CompleteAsync(lockToken);
        }

        static Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            System.Diagnostics.Debug.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            System.Diagnostics.Debug.WriteLine("Exception context for troubleshooting:");
            System.Diagnostics.Debug.WriteLine($"- Endpoint: {context.Endpoint}");
            System.Diagnostics.Debug.WriteLine($"- Entity Path: {context.EntityPath}");
            System.Diagnostics.Debug.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
