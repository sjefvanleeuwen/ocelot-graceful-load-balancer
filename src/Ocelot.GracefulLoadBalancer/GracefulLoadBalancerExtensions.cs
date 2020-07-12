using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Requester;
using Ocelot.ServiceDiscovery.Providers;
using System;

namespace Ocelot.DependencyInjection
{
    public static class GracefulLoadBalancerExtensions
    {
        public static IOcelotBuilder AddGracefullLoadBalancer(this IOcelotBuilder builder)
        {
            GracefulLoadBalancerDelegatingHandler delegatingHandler = null;
            builder.Services.AddTransient<GracefulLoadBalancerDelegatingHandler>();
            builder.Services.AddTransient(s =>
            {
                delegatingHandler = s.GetService<GracefulLoadBalancerDelegatingHandler>();
                // Make this handler global.
                return new GlobalDelegatingHandler(delegatingHandler);
            });

            Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, GracefulLoadBalancer> loadBalancerFactoryFunc
                = (serviceProvider, route, serviceDiscoveryProvider) =>
                {
                    return new GracefulLoadBalancer(serviceProvider, route, serviceDiscoveryProvider.Get);
                };

            builder.AddCustomLoadBalancer(loadBalancerFactoryFunc);
            return builder;
        }
    }
}
