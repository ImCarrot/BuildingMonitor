namespace BuildingMonitor.Messages
{
    public class RequestFloorIds
    {
        public long RequestId { get; }

        public RequestFloorIds(long requestId)
        {
            RequestId = requestId;
        }
    }
}
