namespace BuildingMonitor.Messages
{
    public sealed class RespondMetadata
    {

        public long RequestId { get; private set; }

        public string FloorId { get; private set; }

        public string SensorId { get; private set; }

        public RespondMetadata(long requestId, string floorId, string sensorId)
        {
            RequestId = requestId;
            FloorId = floorId;
            SensorId = sensorId;
        }

    }
}
