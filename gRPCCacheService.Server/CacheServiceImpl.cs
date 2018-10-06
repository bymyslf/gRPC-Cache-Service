using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using gRPCCaheService.Protos;
using static gRPCCaheService.Protos.CacheService;

namespace gRPCCacheService.Server
{
    public class CacheServiceImpl : CacheServiceBase
    {
        private static readonly Task<GetResponse> _getCompleted = Task.FromResult((GetResponse)null);
        private static readonly Task<SetResponse> _setCompleted = Task.FromResult(new SetResponse());

        public static readonly ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();

        private readonly ILogger _logger;

        public CacheServiceImpl(ILogger logger)
        {
            _logger = logger;
        }

        public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            var correlationId = context.GetCorrelationId();

            _cache.TryGetValue(request.Key, out byte[] value);

            if (value == null)
            {
                _logger.Error($"[{correlationId}] - Could not get '{request.Key}'");
                context.SetCorrelationId(correlationId);
                context.Status = new Status(StatusCode.NotFound, $"Key '{request.Key}' not found");
                return _getCompleted;
            }

            _logger.Info($"[{correlationId}] - Get '{request.Key}'");
            context.SetCorrelationId(correlationId);
            return Task.FromResult(new GetResponse { Key = request.Key, Value = ByteString.CopyFrom(value) });
        }

        public override Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            var correlationId = context.GetCorrelationId();

            if (!_cache.TryAdd(request.Key, request.Value.ToByteArray()))
            {
                _logger.Error($"[{correlationId}] - Could not set '{request.Key}'");
                context.Status = new Status(StatusCode.Internal, $"Could not set '{request.Key}'");
            }

            _logger.Info($"[{correlationId}] - Set '{request.Key}'");
            context.SetCorrelationId(correlationId);
            return _setCompleted;
        }
    }
}