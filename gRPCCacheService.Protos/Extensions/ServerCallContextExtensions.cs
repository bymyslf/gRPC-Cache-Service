using Grpc.Core;
using System;
using System.Linq;

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

        public static void SetCorrelationId(this ServerCallContext context, string value)
            => context.ResponseTrailers.Add(CorrelationIdHeader, value);
    }
}