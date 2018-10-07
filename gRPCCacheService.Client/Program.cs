using Grpc.Core;
using System.Threading.Tasks;
using gRPCCaheService.Protos;
using Google.Protobuf;
using System.Text;
using Grpc.Core.Logging;
using static System.Console;
using static gRPCCaheService.Protos.CacheService;
using static Grpc.Core.GrpcEnvironment;
using System.Collections.Generic;
using Grpc.Core.Interceptors;
using gRPCCacheService.Common.Interceptors;
using gRPCCacheService.Common.Auth;
using IdentityModel.Client;
using System;

namespace gRPCCacheService.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            SetLogger(new ConsoleLogger());

            //Only for testing purposes
            var options = new List<ChannelOption>
            {
                new ChannelOption(ChannelOptions.SslTargetNameOverride, "foo.test.google.fr")
            };

            var token = await GetToken();
            var credentials = ChannelCredentials.Create(Credentials.CreateSslClientCredentials(), IdSvrGrpcCredentials.ToCallCredentials(token));
            var channel = new Channel("localhost", 5000, credentials, options);
            var invoker = channel.Intercept(new CorrelationIdInterceptor());
            await channel.ConnectAsync();

            var client = new CacheServiceClient(invoker);

            try
            {
                var response = await client.SetAsync(new SetRequest
                {
                    Key = "ClientDemo",
                    Value = ByteString.CopyFrom("ClientDemo", Encoding.UTF8)
                }, options: new CallOptions().WithDeadline(DateTime.UtcNow.AddSeconds(2)));

                Logger.Info("Set key 'ClientDemo'");
            }
            catch (RpcException ex)
            {
                Logger.Error(ex, "Error setting key: 'ClientDemo'");
            }

            ReadLine();

            await channel.ShutdownAsync();
        }

        private static async Task<TokenResponse> GetToken()
        {
            var disco = await DiscoveryClient.GetAsync(IdentityConstants.Authority);
            if (disco.IsError)
            {
                Logger.Error(disco.Error);
                return null;
            }

            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(IdentityConstants.Audience);

            if (tokenResponse.IsError)
            {
                Logger.Error(tokenResponse.Error);
                return null;
            }

            return tokenResponse;
        }
    }
}