# Healthchecks in .NET

Healthchecks are a common way to monitor the health of a service. They are typically implemented as a HTTP endpoint that returns a 200 OK response if the service is healthy, but for non-HTTP services like a worker service healthchecks can be done over TCP or as a file write. Healthchecks are often used in Kubernetes to determine if a pod should be restarted.

## Implementation in .NET

For ASP.NET Core applications (typically web apps or APIs) healthchecks are implemented as a simple HTTP endpoint. It can often look like:

```csharp
app.MapGet("/health", () => HealthCheckResult.Healthy());
```

This is the most trivial implementation, but it can work as a simple check that the app is running and responding to HTTP requests.

For e.g. worker services that don't have an HTTP endpoint healthchecks can be implemented with an HttpListener or a small Kestrel server - or for simpler variations with a file write or a TCP check. They can also just use the Microsoft.NET.Sdk.Web similar to the ASP.NET Core apps at a cost of a bit of extra memory usage (~30MB per instance).

## Common pitfalls

Deep healthchecks have been the cause of multiple well documented outages like [DoorDash](https://careersatdoordash.com/blog/how-to-handle-kubernetes-health-checks/).

Generally pitfalls are often variations over using them inappropriately. That is, often health checks are used by Kubernetes to determine if a pod should be restarted, but then they are implemented in a way like checking a database connection - but a slow database won't necessarily be helped by restarting the pod. So determine the purpose of your healthcheck and implement it accordingly.

## Types of healthchecks

For kubernetes specifically there are three types of healthchecks:

1. **Liveness** - This is to determine if the pod is running or not. If this fails the pod will be restarted.
2. **Readiness** - This is to determine if the pod is ready to accept traffic and as such mostly applies to web apps or APIs. If the pod can't connect to the database, it might not help to restart the pod, but it also won't help to direct traffic to it. So here it might make sense to make deep healthchecks or e.g. try to reroute traffic if just responses on this pod are failing or slow.
3. **Startup** - This is to determine if the pod has completed its startup sequence and is ready to accept traffic. It mostly makes sense if e.g. caches need to be warmed up or other long running tasks need to complete before requests can be processed.

But there are also other way to use healthchecks. A common one is to just make a service overview page where one can then dive in and see that service x has health=degraded and that the cause is that the database is slow. In this case deep healthchecks can also make sense to explain the current health.

## Useful healthchecks

* A simple ping check can be good to just see if the service is running and responding to requests.
* A database connection check can make sense, especially for readiness checks.
* A stream or queue check, e.g. checking that we can successfully write and read from a queue or stream, can make sense for readiness checks.
* It can also make sense to check on queue depth or stream position to see if we are falling behind - again this is a case where a restart might not make much sense, but surfacing this as a degraded state (or alternatively alerting on it) can be useful.
* It might also make sense to have checks for core infrastructure - not inside specific services, but as separate healthchecks. Vpn, file systems, database, the kubernetes cluster, the logging setup, NGinx (home pages being reachable from the outside), build pipeline, etc.

## Asp.Net Core Healthchecks.UI

See especially this Tim Corey video: [Health Checks in ASP.NET Core](https://www.youtube.com/watch?v=Kbfto6Y2xdw).
See the package here: [AspNetCore.HealthChecks.UI](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI/) and the site here: [Healthchecks.UI](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks).

## More reading

* [Health Checks in Distributed Systems](https://copyconstruct.medium.com/health-checks-in-distributed-systems-aa8a0e8c1672)
* [Horror Stories: Kubernetes Deep Health Checks](https://encore.dev/blog/horror-stories-k8s)
