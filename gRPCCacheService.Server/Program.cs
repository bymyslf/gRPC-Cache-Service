using Grpc.Core;
using Grpc.Core.Logging;
using gRPCCaheService.Protos;
using System.Threading.Tasks;
using static Grpc.Core.GrpcEnvironment;

namespace gRPCCacheService.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            SetLogger(new ConsoleLogger());

            var server = new Grpc.Core.Server
            {
                Ports = { { "localhost", 5000, ServerCredentials.Insecure } },
                Services = { CacheService.BindService(new CacheServiceImpl(Logger)) }
            };

            server.Start();

            await server.ShutdownTask;
        }
    }
}