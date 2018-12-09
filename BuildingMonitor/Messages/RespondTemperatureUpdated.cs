namespace BuildingMonitor.Messages
{
    public sealed class RespondTemperatureUpdated
    {
        public long RequestId { get; private set; }

        public RespondTemperatureUpdated(long requestId)
        {
            RequestId = requestId;
        }
    }
}
