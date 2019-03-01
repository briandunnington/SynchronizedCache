using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SynchronizedCacheExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // TODO: add your Service Bus connection string here (or, in real-life, read it from KeyVault or something secure)
            Configuration["ServiceBusConnectionString"] = "Endpoint=sb://YOUR_NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY";

            var cacheServiceBusManagementClient = new ManagementClient(Configuration["ServiceBusConnectionString"]);
            var cacheTopicClient = new TopicClient(Configuration["ServiceBusConnectionString"], "synchronizedcache");

            services.AddSingleton<ManagementClient>((p) => cacheServiceBusManagementClient);
            services.AddSingleton<ITopicClient>((p) => cacheTopicClient);
            services.AddSingleton<IAnimalCache, AnimalCache>();
            services.AddSingleton<ICarCache, CarCache>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
