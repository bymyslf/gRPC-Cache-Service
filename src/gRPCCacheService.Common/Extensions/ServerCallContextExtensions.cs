using Grpc.Core;
using gRPCCacheService.Common.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gRPCCacheService.Common.Extensions
{
    public static class ServerCallContextExtensions
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public static string GetCorrelationId(this ServerCallContext context)
        {
            var authHeader = context.RequestHeaders.FirstOrDefault(
                h => h.Key.Equals(CorrelationIdHeader, StringComparison.OrdinalIgnoreCase));
            if (authHeader != null)
                return authHeader.Value;

            return null;
        }

        public static Task SetCorrelationId(this ServerCallContext context, string value)
        {
            var metadata = new Metadata();
            metadata.Add(CorrelationIdHeader, value);
            return context.WriteResponseHeadersAsync(metadata);
        }

        public static string GetAccessToken(this ServerCallContext context)
        {
            var authHeader = context.RequestHeaders.FirstOrDefault(h => h.Key.Equals(IdSvrAuthInterceptors.AuthorizationHeader, StringComparison.OrdinalIgnoreCase));
            if (authHeader != null)
                return authHeader.Value.Substring(IdSvrAuthInterceptors.Schema.Length).Trim();

            return null;
        }
    }
}