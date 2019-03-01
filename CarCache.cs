using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynchronizedCacheExample
{
    public interface ICarCache : ISynchronizedCache<Car, CarType>
    {
    }

    public class CarCache : SynchronizedCache<Car, CarType>, ICarCache
    {
        public CarCache(ManagementClient serviceBusManagementClient, ITopicClient serviceBusTopicClient) : base(serviceBusManagementClient, serviceBusTopicClient)
        {
        }

        protected override string GetHashKey(CarType carType)
        {
            return $"{carType.Make}|{carType.Model}";
        }

        protected override async Task<Car> Load(CarType carType)
        {
            // Pretend you loaded this from a DB or some other async place
            await Task.Delay(5000);
            return new Car() { Make = carType.Make, Model = carType.Model, Horsepower = (DateTime.Now.Second * 100), MSRP = (decimal)(DateTime.Now.TimeOfDay.TotalMinutes + 10000) };
        }
    }
}
