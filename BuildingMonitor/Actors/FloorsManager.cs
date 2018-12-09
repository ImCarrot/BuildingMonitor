using Akka.Actor;
using BuildingMonitor.Messages;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BuildingMonitor.Actors
{
    public class FloorsManager : UntypedActor
    {

        private Dictionary<string, IActorRef> floorMap = new Dictionary<string, IActorRef>();

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestRegisterTemperatureSensor m:

                    if (!floorMap.ContainsKey(m.FloorId))
                    {
                        var floor = Context.ActorOf(Floor.Prop(m.FloorId), $"floor-{m.FloorId}");
                        floorMap.Add(m.FloorId, floor);
                        Context.Watch(floor);
                    }

                    floorMap[m.FloorId].Forward(m);

                    break;

                case RequestFloorIds m:
                    Sender.Tell(new RespondFloorIds(m.RequestId, ImmutableHashSet.CreateRange(floorMap.Keys)));
                    break;
                case Terminated m:
                    floorMap.Remove(floorMap.First(x => x.Value == m.ActorRef).Key);
                    break;
                default:
                    break;
            }
        }

        public static Props Prop() =>
            Props.Create<FloorsManager>();
    }
}
