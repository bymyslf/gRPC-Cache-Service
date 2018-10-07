using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using gRPCCacheService.Common.Extensions;

namespace gRPCCacheService.Common.Interceptors
{
    public class LoggingInterceptor : Interceptor
    {
        private readonly ILogger _logger;

        public LoggingInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var response = await base.UnaryServerHandler(request, context, continuation);

            var correlationId = context.GetCorrelationId();

            if (context.Status.StatusCode.Equals(StatusCode.NotFound)
                || context.Status.StatusCode.Equals(StatusCode.Internal))
            {
                _logger.Error($"[{correlationId}] - Error request '{request.ToString()}'");
            }
            else
            {
                _logger.Info($"[{correlationId}] - Request success '{request.ToString()}'");
            }

            return response;
        }
    }
}