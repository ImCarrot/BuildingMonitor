using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingMonitor.Messages
{
    public sealed class RespondSensorRegistered
    {
        public long RequestId { get; private set; }

        public IActorRef SensorReference { get; private set; }

        public RespondSensorRegistered(long requestId, IActorRef sensorRef)
        {
            RequestId = requestId;
            SensorReference = sensorRef;
        }

    }
}
