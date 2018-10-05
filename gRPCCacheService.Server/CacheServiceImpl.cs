using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using gRPCCaheService.Protos;
using static gRPCCaheService.Protos.CacheService;

namespace gRPCCacheService.Server
{
    public class CacheServiceImpl : CacheServiceBase
    {
        private static readonly Task<SetResponse> _setCompleted = Task.FromResult(new SetResponse());
        public static readonly ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();

        public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            _cache.TryGetValue(request.Key, out byte[] value);
            return Task.FromResult(new GetResponse { Key = request.Key, Value = value });
        }

        public override Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            _cache.TryAdd(request.Key, request.Value);
            return _setCompleted;
        }
    }
}