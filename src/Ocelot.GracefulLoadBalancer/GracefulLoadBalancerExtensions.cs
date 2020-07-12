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
            GracefulLoadBalancer balancer = null;
            GracefulLoadBalancerDelegatingHandler delegatingHandler = null;
            builder.Services.AddTransient<GracefulLoadBalancerDelegatingHandler>();
            // Make this handler global.
            builder.Services.AddTransient(s =>
            {
                delegatingHandler = s.GetService<GracefulLoadBalancerDelegatingHandler>();
                delegatingHandler.balancer = balancer;
                return new GlobalDelegatingHandler(delegatingHandler);
            });

            Func<IServiceProvider, DownstreamRoute, IServiceDiscoveryProvider, GracefulLoadBalancer> loadBalancerFactoryFunc
                = (serviceProvider, Route, serviceDiscoveryProvider) =>
                {
                    balancer = new GracefulLoadBalancer(delegatingHandler, serviceDiscoveryProvider.Get);
                    return balancer;
                };

            builder.AddCustomLoadBalancer(loadBalancerFactoryFunc);
            return builder;
        }
    }
}
