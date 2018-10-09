using Grpc.Core;
using IdentityModel.Client;

namespace gRPCCacheService.Common.Auth
{
    public static class IdSvrGrpcCredentials
    {
        public static CallCredentials FromAccessToken(string accessToken)
            => CallCredentials.FromInterceptor(IdSvrAuthInterceptors.FromAccessToken(accessToken));

        public static CallCredentials ToCallCredentials(this TokenResponse token)
            => CallCredentials.FromInterceptor(IdSvrAuthInterceptors.FromToken(token));

        public static ChannelCredentials ToChannelCredentials(this TokenResponse token)
            => ChannelCredentials.Create(new SslCredentials(), token.ToCallCredentials());
    }
}