using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Morstad.ApiGateway.Cli
{

    class Program
    {
        public static void Main(string[] args)
        {
           new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config
                    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                    .AddYamlFile("ocelot.yaml")
                    .AddEnvironmentVariables();
            })
            .ConfigureServices(s => {
                s.AddOcelot().AddGracefullLoadBalancer();
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                   //add your logging
               })
            .UseIISIntegration()
            .Configure(app =>
            {
                app.UseOcelot().Wait();
            })
            .Build()
            .Run();
        }
    }
}
