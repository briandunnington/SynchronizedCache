using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynchronizedCacheExample
{
    public class Car
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Horsepower { get; set; }
        public decimal MSRP { get; set; }
    }

    public class CarType
    {
        public string Make { get; set; }
        public string Model { get; set; }
    }
}
