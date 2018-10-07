using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using gRPCCacheService.Common.Extensions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace gRPCCacheService.Common.Auth
{
    public class JwtValidationInterceptor : Interceptor
    {
        private readonly ILogger _logger;

        public JwtValidationInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            if (!(await IsJwtValid(context.GetAccessToken(), _logger)))
            {
                context.Status = new Status(StatusCode.Unauthenticated, "Invalid token");
                return default(TResponse);
            }

            return await continuation(requestStream, context);
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            if (!(await IsJwtValid(context.GetAccessToken(), _logger)))
            {
                context.Status = new Status(StatusCode.Unauthenticated, "Invalid token");
                return;
            }

            await continuation(request, responseStream, context);
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            if (!(await IsJwtValid(context.GetAccessToken(), _logger)))
            {
                context.Status = new Status(StatusCode.Unauthenticated, "Invalid token");
                return default(TResponse);
            }

            return await continuation(request, context);
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            if (!(await IsJwtValid(context.GetAccessToken(), _logger)))
            {
                context.Status = new Status(StatusCode.Unauthenticated, "Invalid token");
                return;
            }

            await continuation(requestStream, responseStream, context);
        }

        public static async Task<bool> IsJwtValid(string jwt, ILogger logger)
        {
            var disco = await DiscoveryClient.GetAsync(IdentityConstants.Authority);
            if (disco.IsError)
            {
                logger.Error(disco.Error);
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            try
            {
                var keys = new List<SecurityKey>();
                foreach (var webKey in disco.KeySet.Keys)
                {
                    var e = Base64Url.Decode(webKey.E);
                    var n = Base64Url.Decode(webKey.N);

                    var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                    {
                        KeyId = webKey.Kid
                    };

                    keys.Add(key);
                }

                var parameters = new TokenValidationParameters
                {
                    ValidIssuer = disco.Issuer,
                    ValidAudience = IdentityConstants.Audience,
                    IssuerSigningKeys = keys,
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };

                tokenHandler.ValidateToken(jwt, parameters, out validatedToken);
            }
            catch (SecurityTokenException e)
            {
                return false;
            }
            catch (Exception e)
            {
                logger.Error(e, e.ToString());
                throw;
            }

            return validatedToken != null;
        }
    }
}