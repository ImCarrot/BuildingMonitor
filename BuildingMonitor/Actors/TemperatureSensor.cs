using Akka.Actor;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
    public class TemperatureSensor : UntypedActor
    {

        private readonly string floorId;
        private readonly string sensorId;
        private double? lastKnownTemperature;


        public TemperatureSensor(string floorID, string sensorID)
        {
            this.floorId = floorID;
            this.sensorId = sensorID;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestMetadata m:
                    Sender.Tell(new RespondMetadata(m.RequestId, floorId, sensorId));
                    break;
                case RequestTemperature m:
                    Sender.Tell(new RespondTemperature(m.RequestId, lastKnownTemperature));
                    break;
                case RequestUpdateTemperature m:
                    lastKnownTemperature = m.Temperature;
                    Sender.Tell(new RespondTemperatureUpdated(m.RequestId));
                    break;
                case RequestRegisterTemperatureSensor m when m.FloorId == floorId && m.SensorId == sensorId:
                    Sender.Tell(new RespondSensorRegistered(m.RequestId, Context.Self));
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Prop(string floorID, string sensorID)
        {
            return Props.Create(() => new TemperatureSensor(floorID, sensorID));
        }
    }
}
