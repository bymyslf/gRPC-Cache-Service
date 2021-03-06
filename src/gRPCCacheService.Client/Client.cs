﻿using Grpc.Core;
using System.Threading.Tasks;
using gRPCCaheService.Protos;
using Google.Protobuf;
using System.Text;
using Grpc.Core.Logging;
using static System.Console;
using static gRPCCaheService.Protos.CacheService;
using static Grpc.Core.GrpcEnvironment;
using System.Collections.Generic;
using Grpc.Core.Interceptors;
using gRPCCacheService.Common.Interceptors;
using gRPCCacheService.Common.Auth;
using IdentityModel.Client;
using System;
using gRPCCacheService.Common;

namespace gRPCCacheService.Client
{
    internal class Client
    {
        private static async Task Main(string[] args)
        {
            SetLogger(new ConsoleLogger());

            //Only for testing purposes
            var options = new List<ChannelOption>
            {
                new ChannelOption(ChannelOptions.SslTargetNameOverride, "foo.test.google.fr")
            };

            var token = await GetToken();
            var credentials = ChannelCredentials.Create(Credentials.CreateSslClientCredentials(), IdSvrGrpcCredentials.ToCallCredentials(token));
            var channel = new Channel("localhost", 5000, credentials, options);
            var invoker = channel.Intercept(new CorrelationIdInterceptor()).Intercept(new LoggingInterceptor(Logger));
            await channel.ConnectAsync();

            await GeneratedClientExample(invoker);
            await CustomSerializerExample(invoker);

            ReadLine();

            await channel.ShutdownAsync();
        }

        private static async Task GeneratedClientExample(CallInvoker invoker)
        {
            var client = new CacheServiceClient(invoker);

            try
            {
                await client.SetAsync(new SetRequest
                {
                    Key = "ClientDemo",
                    Value = ByteString.CopyFrom("ClientDemo", Encoding.UTF8)
                }, options: new CallOptions().WithDeadline(DateTime.UtcNow.AddSeconds(2)));

                var response = await client.GetAsync(new GetRequest
                {
                    Key = "ClientDemo",
                }, options: new CallOptions().WithDeadline(DateTime.UtcNow.AddSeconds(2)));

                Logger.Info("Get by key pattern 'Client'");

                using (var stream = client.GetByKeyPattern(new GetByKeyPatternRequest
                {
                    Pattern = "Client",
                }, options: new CallOptions().WithDeadline(DateTime.UtcNow.AddSeconds(5))))
                {
                    while (await stream.ResponseStream.MoveNext())
                    {
                        var current = stream.ResponseStream.Current;
                        Logger.Info($"Got key '{current.Key}' with value '{current.Value.ToStringUtf8()}'");
                    }
                };
            }
            catch (RpcException ex)
            {
                Logger.Error(ex, "Error setting key: 'ClientDemo'");
            }
        }

        private static async Task CustomSerializerExample(CallInvoker invoker)
        {
            try
            {
                await invoker.AsyncUnaryCall(
                    Descriptors.SetAsJsonMethod,
                    null,
                    new CallOptions().WithDeadline(DateTime.UtcNow.AddSeconds(2)),
                    new SetAsJsonRequest
                    {
                        Key = "ClientDemoJson",
                        Value = Encoding.UTF8.GetBytes("ClientDemoJson")
                    });

                var json = await invoker.AsyncUnaryCall(
                    Descriptors.GetAsJsonMethod,
                    null,
                    new CallOptions().WithDeadline(DateTime.UtcNow.AddSeconds(2)),
                    new GetAsJsonRequest
                    {
                        Key = "ClientDemoJson",
                    });
            }
            catch (RpcException ex)
            {
                Logger.Error(ex, "Error setting key: 'ClientDemoJson'");
            }
        }

        private static async Task<TokenResponse> GetToken()
        {
            var disco = await DiscoveryClient.GetAsync(IdentityConstants.Authority);
            if (disco.IsError)
            {
                Logger.Error(disco.Error);
                return null;
            }

            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(IdentityConstants.Audience);

            if (tokenResponse.IsError)
            {
                Logger.Error(tokenResponse.Error);
                return null;
            }

            return tokenResponse;
        }
    }
}