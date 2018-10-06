using Grpc.Core;
using System.Threading.Tasks;
using gRPCCaheService.Protos;
using Google.Protobuf;
using System.Text;
using Grpc.Core.Logging;
using static System.Console;
using static gRPCCaheService.Protos.CacheService;
using static Grpc.Core.GrpcEnvironment;
using System;
using gRPCCacheService.Common;
using System.Collections.Generic;

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
            await channel.ConnectAsync();

            var client = new CacheServiceClient(channel);

            try
            {
                var metadata = new Metadata { { "X-Correlation-ID", $"{Guid.NewGuid()}" } };
                var response = await client.SetAsync(new SetRequest
                {
                    Key = "ClientDemo",
                    Value = ByteString.CopyFrom("ClientDemo", Encoding.UTF8)
                }, headers: metadata);

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