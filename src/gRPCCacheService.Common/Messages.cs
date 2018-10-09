namespace gRPCCacheService.Common
{
    public class GetAsJsonRequest
    {
        public string Key { get; set; }
    }

    public class GetAsJsonResponse
    {
        public string Key { get; set; }

        public byte[] Value { get; set; }
    }

    public class SetAsJsonRequest
    {
        public string Key { get; set; }

        public byte[] Value { get; set; }
    }

    public class SetAsJsonResponse { }
}