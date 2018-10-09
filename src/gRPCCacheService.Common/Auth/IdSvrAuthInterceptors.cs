using Grpc.Core;
using Grpc.Core.Utils;
using IdentityModel.Client;
using System.Threading.Tasks;

namespace gRPCCacheService.Common.Auth
{
    public static class IdSvrAuthInterceptors
    {
        public const string AuthorizationHeader = "Authorization";
        public const string Schema = "Bearer";

        public static AsyncAuthInterceptor FromToken(TokenResponse token)
        {
            GrpcPreconditions.CheckNotNull(token);
            return new AsyncAuthInterceptor((context, metadata) =>
            {
                metadata.Add(CreateBearerTokenHeader(token.AccessToken));
                return Task.CompletedTask;
            });
        }

        public static AsyncAuthInterceptor FromAccessToken(string accessToken)
        {
            GrpcPreconditions.CheckNotNull(accessToken);
            return new AsyncAuthInterceptor((context, metadata) =>
            {
                metadata.Add(CreateBearerTokenHeader(accessToken));
                return Task.CompletedTask;
            });
        }

        private static Metadata.Entry CreateBearerTokenHeader(string accessToken)
            => new Metadata.Entry(AuthorizationHeader, Schema + " " + accessToken);
    }
}