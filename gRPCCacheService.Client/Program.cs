using Grpc.Core;
using System.Threading.Tasks;
using static System.Console;
using static gRPCCaheService.Protos.CacheService;
using gRPCCaheService.Protos;

namespace gRPCCacheService.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var channel = new Channel("localhost", 5000, ChannelCredentials.Insecure);
            await channel.ConnectAsync();

            var client = new CacheServiceClient(channel);
            var response = client.SetAsync(new SetRequest { Key = "ClientDemo", Value = "ClientDemo" });

            ReadLine();

            await channel.ShutdownAsync();
        }
    }
}