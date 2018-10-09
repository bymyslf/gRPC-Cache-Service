using Grpc.Core;
using gRPCCaheService.Protos;

namespace gRPCCacheService.Common
{
    public static class Descriptors
    {
        public static Method<GetAsJsonRequest, GetAsJsonResponse> GetAsJsonMethod = new Method<GetAsJsonRequest, GetAsJsonResponse>(
                    type: MethodType.Unary,
                    serviceName: CacheService.Descriptor.Name,
                    name: "GetAsJson",
                    requestMarshaller: Marshallers.Create(
                        serializer: Serializer<GetAsJsonRequest>.ToBytes,
                        deserializer: Serializer<GetAsJsonRequest>.FromBytes
                        ),
                    responseMarshaller: Marshallers.Create(
                        serializer: Serializer<GetAsJsonResponse>.ToBytes,
                        deserializer: Serializer<GetAsJsonResponse>.FromBytes
                        )
            );

        public static Method<SetAsJsonRequest, SetAsJsonResponse> SetAsJsonMethod = new Method<SetAsJsonRequest, SetAsJsonResponse>(
                    type: MethodType.Unary,
                    serviceName: CacheService.Descriptor.Name,
                    name: "SetAsJson",
                    requestMarshaller: Marshallers.Create(
                        serializer: Serializer<SetAsJsonRequest>.ToBytes,
                        deserializer: Serializer<SetAsJsonRequest>.FromBytes
                        ),
                    responseMarshaller: Marshallers.Create(
                        serializer: Serializer<SetAsJsonResponse>.ToBytes,
                        deserializer: Serializer<SetAsJsonResponse>.FromBytes
                        )
            );
    }
}