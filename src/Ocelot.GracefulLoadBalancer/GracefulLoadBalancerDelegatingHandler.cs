using Ocelot.Values;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class GracefulLoadBalancerDelegatingHandler : DelegatingHandler
    {
        internal GracefulLoadBalancer balancer { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //request.RequestUri = new Uri("http://192.168.178.90:8800/api/landelijke_tabellen/tabellen/1/waarden");
            HttpResponseMessage response = null;
            try
            {
                //do stuff and optionally call the base handler..
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine("Marking as bad: " + request.RequestUri.Host);
                    await balancer.MarkAsBad(new ServiceHostAndPort(request.RequestUri.Host,request.RequestUri.Port,request.RequestUri.Scheme));
                    // Get a new lease from the balancer
                    var s = await balancer.Lease(null); /* No Http Context needed for the balancer */
                    request.RequestUri = new Uri($"{s.Data.Scheme}://{s.Data.DownstreamHost}:{s.Data.DownstreamPort}/{request.RequestUri.PathAndQuery}");

                    response = await base.SendAsync(request, cancellationToken);
                }
                catch (Exception e1)
                {

                }
            }
            return response;
        }
    }
}
