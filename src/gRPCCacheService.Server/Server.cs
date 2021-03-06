﻿using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using gRPCCacheService.Common;
using gRPCCacheService.Common.Auth;
using gRPCCacheService.Common.Interceptors;
using gRPCCaheService.Protos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using static Grpc.Core.GrpcEnvironment;

namespace gRPCCacheService.Server
{
    internal class Server
    {
        private static async Task Main(string[] args)
        {
            SetLogger(new ConsoleLogger());

            var service = new CacheServiceImpl(Logger);
            var server = new Grpc.Core.Server
            {
                Ports = { { "localhost", 5000, Credentials.CreateSslServerCredentials() } },
                Services =
                {
                    CacheService.BindService(service)
                        .Intercept(new CorrelationIdInterceptor())
                        .Intercept(new JwtValidationInterceptor(Logger))
                        .Intercept(new LoggingInterceptor(Logger)),

                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(Descriptors.GetAsJsonMethod, service.GetAsJson)
                        .AddMethod(Descriptors.SetAsJsonMethod, service.SetAsJson)
                        .Build()
                        .Intercept(new CorrelationIdInterceptor())
                        .Intercept(new JwtValidationInterceptor(Logger))
                        .Intercept(new LoggingInterceptor(Logger))
                }
            };

            server.Start();

            var webHost = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:60000")
                .Build();

            await webHost.RunAsync();
        }

        private class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<GrpcRestGatewayMiddleware>(new Channel("localhost:5000", Credentials.CreateSslClientCredentials()));
            }
        }
    }
}