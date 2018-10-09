using Grpc.Core;
using System.IO;
using System.Reflection;

namespace gRPCCacheService.Common.Auth
{
    public static class Credentials
    {
        public static string ClientCertAuthorityPath => GetPath("data/ca.pem");

        public static string ServerCertChainPath => GetPath("data/server1.pem");

        public static string ServerPrivateKeyPath => GetPath("data/server1.key");

        public static SslCredentials CreateSslClientCredentials()
           => new SslCredentials(File.ReadAllText(ClientCertAuthorityPath));

        public static SslServerCredentials CreateSslServerCredentials()
        {
            var keyCertPair = new KeyCertificatePair(
                File.ReadAllText(ServerCertChainPath),
                File.ReadAllText(ServerPrivateKeyPath));
            return new SslServerCredentials(new[] { keyCertPair });
        }

        private static string GetPath(string relativePath)
        {
            var assemblyDir = Path.GetDirectoryName(typeof(Credentials).GetTypeInfo().Assembly.Location);
            return Path.Combine(assemblyDir, relativePath);
        }
    }
}