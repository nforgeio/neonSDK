## Introduction
This package contains some useful abstractions for SignalR services.

## SignalR Proxy
The SignalR Proxy enables sticky sessions for SignalR clients.
It removes the need for a load balancer in front of your service.
The proxy is a middleware that intercepts all requests to the SignalR hub and redirects them to the correct instance.
This works by storing the upstream host in a cookie, allowing the backend service to be scaled up/down without causing clients to be disconnected which would happen with a hashing load balancer.

For this to work, the `PeerAddress` must be an `SRV` record.
In Kubernetes this can be achieved by using a [headless service](https://kubernetes.io/docs/concepts/services-networking/service/#headless-services).

### Configuration
```
builder.Services.AddSignalrProxy(options =>
{
    options.PeerAddress = "my-headless-service.ns.svc.cluster.local";
    options.Port        = PORT;
});

app.UseSignalrProxy();
```

## NATS backplane
The NATS bakplane enables a distributed SignalR service, using NATS as the message broker.

### Configuration
```
builder.Services.AddSignalR()
    .AddNats(options =>
    {
        options.Servers = new string[] { "nats.ns.svc.cluster.local" };
    });
```