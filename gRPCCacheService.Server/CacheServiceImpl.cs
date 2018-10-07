using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using gRPCCacheService.Common;
using gRPCCaheService.Protos;
using static gRPCCaheService.Protos.CacheService;

namespace gRPCCacheService.Server
{
    public class CacheServiceImpl : CacheServiceBase
    {
        private static readonly Task<GetResponse> _getCompleted = Task.FromResult(new GetResponse());
        private static readonly Task<GetAsJsonResponse> _getJsonCompleted = Task.FromResult(new GetAsJsonResponse());
        private static readonly Task<SetResponse> _setCompleted = Task.FromResult(new SetResponse());
        private static readonly Task<SetAsJsonResponse> _setJsonCompleted = Task.FromResult(new SetAsJsonResponse());

        public static readonly ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();

        private readonly ILogger _logger;

        public CacheServiceImpl(ILogger logger)
        {
            _logger = logger;
        }

        public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            var value = this.GetInternal(request.Key, context);
            if (value == null)
            {
                return _getCompleted;
            }

            return Task.FromResult(new GetResponse { Key = request.Key, Value = ByteString.CopyFrom(value) });
        }

        public override Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            this.SetInternal(request.Key, request.Value.ToByteArray(), context);
            return _setCompleted;
        }

        public Task<GetAsJsonResponse> GetAsJson(GetAsJsonRequest request, ServerCallContext context)
        {
            var value = this.GetInternal(request.Key, context);
            if (value == null)
            {
                return _getJsonCompleted;
            }

            return Task.FromResult(new GetAsJsonResponse { Key = request.Key, Value = value });
        }

        public Task<SetAsJsonResponse> SetAsJson(SetAsJsonRequest request, ServerCallContext context)
        {
            this.SetInternal(request.Key, request.Value, context);
            return _setJsonCompleted;
        }

        private byte[] GetInternal(string key, ServerCallContext context)
        {
            _cache.TryGetValue(key, out byte[] value);

            if (value == null)
            {
                context.Status = new Status(StatusCode.NotFound, $"Key '{key}' not found");
            }

            return value;
        }

        private void SetInternal(string key, byte[] value, ServerCallContext context)
        {
            if (!_cache.TryAdd(key, value))
            {
                context.Status = new Status(StatusCode.Internal, $"Could not set '{key}'");
            }
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