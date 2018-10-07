using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using gRPCCacheService.Common.Extensions;
using gRPCCaheService.Protos;
using static gRPCCaheService.Protos.CacheService;

namespace gRPCCacheService.Server
{
    public class CacheServiceImpl : CacheServiceBase
    {
        private static readonly Task<GetResponse> _getCompleted = Task.FromResult(new GetResponse());
        private static readonly Task<SetResponse> _setCompleted = Task.FromResult(new SetResponse());

        public static readonly ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();

        private readonly ILogger _logger;

        public CacheServiceImpl(ILogger logger)
        {
            _logger = logger;
        }

        public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            //var correlationId = context.GetCorrelationId();

            _cache.TryGetValue(request.Key, out byte[] value);

            if (value == null)
            {
                //context.SetCorrelationId(correlationId);
                context.Status = new Status(StatusCode.NotFound, $"Key '{request.Key}' not found");
                return _getCompleted;
            }

            //context.SetCorrelationId(correlationId);
            return Task.FromResult(new GetResponse { Key = request.Key, Value = ByteString.CopyFrom(value) });
        }

        public override Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            //var correlationId = context.GetCorrelationId();

            if (!_cache.TryAdd(request.Key, request.Value.ToByteArray()))
            {
                context.Status = new Status(StatusCode.Internal, $"Could not set '{request.Key}'");
            }

            //context.SetCorrelationId(correlationId);
            return _setCompleted;
        }

        public override async Task GetByKeyPattern(
            GetByKeyPatternRequest request,
            IServerStreamWriter<GetByKeyPatternResponse> responseStream,
            ServerCallContext context)
        {
            var enumerator = _cache.Where(it => it.Key.StartsWith(request.Pattern)).GetEnumerator();

            // Keep streaming the sequence until the call is cancelled.
            // Use CancellationToken from ServerCallContext to detect the cancellation.
            while (!context.CancellationToken.IsCancellationRequested && enumerator.MoveNext())
            {
                var current = enumerator.Current;
                await responseStream.WriteAsync(new GetByKeyPatternResponse
                {
                    Key = current.Key,
                    Value = ByteString.CopyFrom(current.Value)
                });
            }
        }
    }
}