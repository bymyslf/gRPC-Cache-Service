using Grpc.Core;
using gRPCCaheService.Protos;
using System.Threading.Tasks;

namespace gRPCCacheService.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var server = new Grpc.Core.Server
            {
                Ports = { { "localhost", 5000, ServerCredentials.Insecure } },
                Services = { CacheService.BindService(new CacheServiceImpl()) }
            };

            server.Start();

            await server.ShutdownTask;
        }
    }
}