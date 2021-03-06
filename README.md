# Ocelot Graceful Load Balancer
[![nuget](https://img.shields.io/nuget/v/Ocelot.GracefulLoadBalancer)](https://www.nuget.org/packages/Ocelot.GracefulLoadBalancer/)
![.NET Core](https://github.com/sjefvanleeuwen/ocelot-graceful-load-balancer/workflows/.NET%20Core/badge.svg)
## What is it?

A Provider for Ocelot which provides a Round Robin Load Balancer with API Nodes Health Checking.

## Status

The provider is in alpha stage but should be usable. It has been tested as part of my cluster setup.

This project is open to pull requests.

## Setup

The setup is comparable to other Ocelot providers. Its target framework is netcoreapp3.1.

### Install nuget package

```
dotnet add package Ocelot.GracefulLoadBalancer --version 0.0.2-alpha
```
or visit the nuget for other options on: https://www.nuget.org/packages/Ocelot.GraceFulLoadBalancer

### Adding the provider to your project

The following shows a typical configuration a cluster for a 3 Node API Endpoint setup on the balancer 
and should be easily adaptable to your own settings. I assume you already have basic knowledge of Ocelot.
Note! That I am using the YAML Configuration provider from NetEscapedes in the sample project instead of
configuring via JSON. If you prefer JSON it should be easy to convert it manually or with a YAML to JSON converter.

Consider the following configuration:

```YAML
Routes:
- DownstreamPathTemplate: "/api/auth/{everything}"
  DownstreamScheme: http
  DownstreamHostAndPorts:
  - Host: 192.168.178.88
    Port: 8800
  - Host: 192.168.178.90
    Port: 8800
  - Host: 192.168.178.91
    Port: 8800
  LoadBalancerOptions:
    Type: GracefulLoadBalancer
  UpstreamPathTemplate: "/api/auth/{everything}"
  UpstreamHttpMethod:
  - Get
```

The `LoadBalancer` options simply point to the implementation. You can add the loadbalancer as follows:

```csharp
builder.ConfigureServices(s => {
    s.AddOcelot().AddGracefulLoadBalancer();
})
```

View the sample ApiGateway for starting up an entire gateway.

