using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingMonitor.Messages
{
    public sealed class RequestRegisterTemperatureSensor
    {
        public long RequestId { get; private set; }

        public string FloorId { get; private set; }

        public string SensorId { get; private set; }

        public RequestRegisterTemperatureSensor(long requestId, string floorId, string sensorId)
        {
            RequestId = requestId;
            SensorId = sensorId;
            FloorId = floorId;
        }
    }
}
