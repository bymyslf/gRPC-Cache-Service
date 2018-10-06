using Grpc.Core;
using System.Threading.Tasks;
using gRPCCaheService.Protos;
using Google.Protobuf;
using System.Text;
using Grpc.Core.Logging;
using static System.Console;
using static gRPCCaheService.Protos.CacheService;
using static Grpc.Core.GrpcEnvironment;

namespace gRPCCacheService.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            SetLogger(new ConsoleLogger());

            var channel = new Channel("localhost", 5000, ChannelCredentials.Insecure);
            await channel.ConnectAsync();

            var client = new CacheServiceClient(channel);

            try
            {
                var response = await client.SetAsync(new SetRequest
                {
                    Key = "ClientDemo",
                    Value = ByteString.CopyFrom("ClientDemo", Encoding.UTF8)
                });
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