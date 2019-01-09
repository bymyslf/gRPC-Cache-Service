using System;
using System.Collections.Generic;
using System.Linq;
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

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            _logger.Info($"Begin '{context.Method.Name}' request with '{request.ToString()}'");

            var responseContinuation = continuation(request, context);
            var responseAsync = responseContinuation.ResponseAsync.ContinueWith(responseTask =>
            {
                try
                {
                    var response = responseTask.Result;
                    _logger.Info($"Request to '{context.Method.Name}' with '{request.ToString()}' succeeded!");
                    return response;
                }
                catch (AggregateException ex)
                {
                    _logger.Error($"Request to '{context.Method.Name}' with '{request.ToString()}' failed!", ex.InnerException);
                    throw ex.InnerException;
                }
            });

            var responseHeaderAsync = responseContinuation.ResponseHeadersAsync.ContinueWith(headerTask =>
            {
                var responseHeader = headerTask.Result;
                _logger.Info($"Request to '{request.ToString()}' response headers: '{string.Join(",", (responseHeader as IList<Metadata.Entry>).Select(entry => entry.ToString()))}'");
                return responseHeader;
            });

            return new AsyncUnaryCall<TResponse>(responseAsync, responseHeaderAsync, responseContinuation.GetStatus, responseContinuation.GetTrailers, responseContinuation.Dispose);
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
                _logger.Error($"[{correlationId}] - Request to '{context.Method}' with '{request.ToString()}' succeeded!");
            }
            else
            {
                _logger.Info($"[{correlationId}] - Request to '{context.Method}' with '{request.ToString()}' failed");
            }

            return response;
        }
    }
}