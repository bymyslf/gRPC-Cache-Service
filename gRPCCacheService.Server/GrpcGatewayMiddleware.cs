using Grpc.Core;
using gRPCCaheService.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static gRPCCaheService.Protos.CacheService;

namespace gRPCCacheService.Server
{
    public class GrpcGatewayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Channel _channel;
        private readonly string _path = $"/cache";

        public GrpcGatewayMiddleware(RequestDelegate next, Channel channel)
        {
            _next = next;
            _channel = channel;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                if (IsGrpcGatewayRequest(httpContext))
                {
                    if (!IsAllowedMethod(httpContext))
                    {
                        httpContext.Response.Clear();
                        httpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        return;
                    }

                    string body;
                    using (var sr = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
                        body = sr.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(body))
                        body = "[]";

                    var lastIndexOf = httpContext.Request.Path.Value.LastIndexOf('/') + 1;
                    var key = new StringSegment(httpContext.Request.Path.Value, lastIndexOf, httpContext.Request.Path.Value.Length - lastIndexOf);

                    var metadata = new Metadata();
                    foreach (var header in httpContext.Request.Headers)
                    {
                        metadata.Add(header.Key, header.Value);
                    }

                    await _channel.ConnectAsync();

                    var client = new CacheServiceClient(_channel);

                    object response;
                    if (HttpMethods.IsGet(httpContext.Request.Method))
                    {
                        var requestObject = JsonConvert.DeserializeObject<SetRequest>(body);
                        response = await client.SetAsync(requestObject, new CallOptions().WithHeaders(metadata));
                    }
                    else
                    {
                        response = await client.GetAsync(new GetRequest { Key = key.ToString() }, new CallOptions().WithHeaders(metadata));
                    }

                    var responseBody = JsonConvert.SerializeObject(response, new[] { new Newtonsoft.Json.Converters.StringEnumConverter() });
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(responseBody);
                    return;
                }
                else
                {
                    await _next(httpContext);
                }
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync(ex.ToString());
            }
        }

        private bool IsGrpcGatewayRequest(HttpContext httpContext)
            => httpContext.Request.Path.Value.StartsWith(_path, StringComparison.OrdinalIgnoreCase);

        private bool IsAllowedMethod(HttpContext httpContext)
            => HttpMethods.IsGet(httpContext.Request.Method) || HttpMethods.IsPut(httpContext.Request.Method);
    }
}