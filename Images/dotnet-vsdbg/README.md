# Image Tags

The latest production image will be tagged with `:latest`.

# Description

**NOTE:** This container image is still incomplete, we need to install the diagnostic tools:

https://thecloudblog.net/post/tracing-and-profiling-a-net-core-application-on-azure-kubernetes-service-with-a-sidecar-container/

This container can be attached to DOTNET application pods as an ephemeral sidecar for remote debugging.
The remote debugger executable is located at:

**/vsdbg/vsdbg**
