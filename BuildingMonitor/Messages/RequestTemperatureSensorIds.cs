namespace BuildingMonitor.Messages
{
    public sealed class RequestTemperatureSensorIds
    {
        public long RequestId { get; private set; }

        public RequestTemperatureSensorIds(long requestId)
        {
            RequestId = requestId;
        }
    }
}
