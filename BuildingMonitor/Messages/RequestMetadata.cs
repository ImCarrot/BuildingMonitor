namespace BuildingMonitor.Messages
{
    public sealed class RequestMetadata
    {

        public long RequestId { get; }

        public RequestMetadata(long requestID)
        {
            RequestId = requestID;
        }

    }
}
