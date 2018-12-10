using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
    public sealed class RespondAllTemeratures
    {
        public long RequestId { get; }

        public IImmutableDictionary<string, ITemperatureQueryReading> TemperatureReadings { get; }

        public RespondAllTemeratures(long requestId, IImmutableDictionary<string, ITemperatureQueryReading> temps)
        {
            RequestId = requestId;
            TemperatureReadings = temps;

        }
    }
}
