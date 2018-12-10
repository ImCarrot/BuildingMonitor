using Akka.Actor;
using BuildingMonitor.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BuildingMonitor.Actors
{
    public class Floor : UntypedActor
    {
        private readonly string floorId;
        private Dictionary<string, IActorRef> sensorMap = new Dictionary<string, IActorRef>();

        public Floor(string floorId)
        {
            this.floorId = floorId;
        }


        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestRegisterTemperatureSensor m when m.FloorId == floorId:

                    if (!sensorMap.ContainsKey(m.SensorId))
                    {
                        var sensor = Context.ActorOf(TemperatureSensor.Prop(floorId, m.SensorId), $"sensor-{m.SensorId}");
                        sensorMap.Add(m.SensorId, sensor);
                        Context.Watch(sensor);
                    }

                    sensorMap[m.SensorId].Forward(m);

                    break;

                case RequestTemperatureSensorIds m:
                    Sender.Tell(new RespondTempratureSenssorIds(m.RequestId, ImmutableHashSet.CreateRange(sensorMap.Keys)));
                    break;
                case RequestAllTemperatures m:
                    var actorRefSensorIdMap = new Dictionary<IActorRef, string>();
                    foreach (var sensor in sensorMap)
                    {
                        actorRefSensorIdMap.Add(sensor.Value, sensor.Key);
                    }
                    Context.ActorOf(FloorQuery.Prop(actorRefSensorIdMap, m.RequestId, Sender, TimeSpan.FromSeconds(3)));
                    break;
                case Terminated m:
                    sensorMap.Remove(sensorMap.First(x => x.Value == m.ActorRef).Key);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Prop(string floorID) =>
            Props.Create(() => new Floor(floorID));
    }
}
