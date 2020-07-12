using Microsoft.AspNetCore.Http;
using Ocelot.Responses;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class GracefulLoadBalancer : ILoadBalancer
    {
        public static Func<Task<List<Service>>> _services;

        private readonly object _lock = new object();
        private Dictionary<int, Service> _failings;
        private Timer t;

        private int _last;
        public GracefulLoadBalancer(GracefulLoadBalancerDelegatingHandler handler, Func<Task<List<Service>>> services)
        {
            _services = services;
        }

        private void Poll(object state)
        {
            Task.Run(() => PollAsync(null));
        }

        public async Task MarkAsBad(ServiceHostAndPort serviceHostAndPort)
        {
            var services = await _services();
            int i = services.FindIndex(p => p.HostAndPort.DownstreamHost == serviceHostAndPort.DownstreamHost && p.HostAndPort.DownstreamPort == serviceHostAndPort.DownstreamPort && p.HostAndPort.Scheme == serviceHostAndPort.Scheme);
            _failings[i] = services[i];
        }

        private async Task PollAsync(object state)
        {
            var services = await _services();
            HttpResponseMessage result = null;
            for (int i = 0; i < services.Count(); i++)
            {
                var r = services[i].HostAndPort;
                try
                {
                    result = await Client.GetAsync(r.Scheme + "://" + r.DownstreamHost + ":" + r.DownstreamPort);
                    if (_failings.ContainsKey(i))
                    {
                        _failings.Remove(i);
                        Console.WriteLine("Info: " + r.Scheme + "://" + r.DownstreamHost + ":" + r.DownstreamPort + " Healthy again.");
                        Console.WriteLine($"Info: available nodes {services.Count - _failings.Count}/{services.Count}");
                    }
                }
                catch (Exception ex)
                {
                    if (!_failings.ContainsKey(i))
                    {
                        _failings[i] = services[i];
                        Console.WriteLine(r.Scheme + "://" + r.DownstreamHost + ":" + r.DownstreamPort + " " + ex.Message);
                        Console.WriteLine($"Info: available nodes {services.Count - _failings.Count}/{services.Count}.");

                    }
                }
            }
        }

        private static HttpClient Client = new HttpClient();

        public async Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            var services = await _services();
            if (_failings is null)
            {
                _failings = new Dictionary<int, Service>();
                await PollAsync(null);
                t = new Timer(Poll);
                t.Change(10000, 10000);
                _last = -1;
            }

            lock (_lock)
            {
                if (_failings.Count == services.Count)
                {
                    Console.WriteLine("Critical: No available api nodes");
                    return new ErrorResponse<ServiceHostAndPort>(new ErrorInvokingLoadBalancerCreator(new Exception("No available API Nodes.")));
                }
                _last++;
                if (_last >= services.Count)
                {
                    _last = 0;
                }
                while (_failings.ContainsKey(_last))
                {
                    _last++;
                    if (_last >= services.Count)
                    {
                        _last = 0;
                    }
                }
                var next = services[_last];

                var response = new OkResponse<ServiceHostAndPort>(next.HostAndPort);
                return response;
            }
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
        }
    }
}

