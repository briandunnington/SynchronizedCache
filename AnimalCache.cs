using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynchronizedCacheExample
{
    public interface IAnimalCache : ISynchronizedCache<Animal, string>
    {
    }

    public class AnimalCache : SynchronizedCache<Animal, string>, IAnimalCache
    {
        public AnimalCache(ManagementClient serviceBusManagementClient, ITopicClient serviceBusTopicClient) : base(serviceBusManagementClient, serviceBusTopicClient)
        {
        }

        protected override string GetHashKey(string key)
        {
            return key;
        }

        protected override async Task<Animal> Load(string key)
        {
            // Pretend you loaded this from a DB or some other async place
            await Task.Delay(5000);
            switch (key)
            {
                case "dog":
                    return new Animal() { Type = key, Name = "Rover", Color = "Brown" };
                case "cat":
                    return new Animal() { Type = key, Name = "Mr. Puddy", Color = "White" };
                case "fish":
                    return new Animal() { Type = key, Name = "Dory", Color = "Blue" };
                default:
                    return null;
            }
        }
    }
}
