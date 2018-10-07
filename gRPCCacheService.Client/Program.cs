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

            var channel = new Channel("localhost", 5000, Credentials.CreateSslClientCredentials(), options);
            var invoker = channel.Intercept(new CorrelationIdInterceptor());
            await channel.ConnectAsync();

            var client = new CacheServiceClient(invoker);

            try
            {
                var response = await client.SetAsync(new SetRequest
                {
                    Key = "ClientDemo",
                    Value = ByteString.CopyFrom("ClientDemo", Encoding.UTF8)
                });

                Logger.Info("Set key 'ClientDemo'");
            }
            catch (RpcException ex)
            {
                Logger.Error(ex, "Error setting key: 'ClientDemo'");
            }

            ReadLine();

            await channel.ShutdownAsync();
        }
    }
}