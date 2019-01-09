using Grpc.Core;
using Grpc.Core.Interceptors;
using gRPCCacheService.Common.Extensions;
using System;
using System.Threading.Tasks;

namespace gRPCCacheService.Common.Interceptors
{
    public class CorrelationIdInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var metadata = context.Options.Headers ?? new Metadata();
            metadata.Add("X-Correlation-ID", $"{Guid.NewGuid()}");
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(metadata));

            return continuation(request, newContext);
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var response = await continuation(request, context);
            await context.SetCorrelationId(context.GetCorrelationId());
            return response;
        }
    }
}