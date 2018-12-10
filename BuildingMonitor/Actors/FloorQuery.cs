using Akka.Actor;
using BuildingMonitor.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace BuildingMonitor.Actors
{
    public class FloorQuery : UntypedActor
    {
        public static readonly long TemperatureRequestCorrelationId = 42;

        private readonly Dictionary<IActorRef, string> actorToSensorId;
        private readonly long requestId;
        private readonly IActorRef requester;
        private readonly TimeSpan timeout;

        private Dictionary<string, ITemperatureQueryReading> repliesReceived = new Dictionary<string, ITemperatureQueryReading>();
        private HashSet<IActorRef> stillAwaitingReply;

        private ICancelable queryTimeoutTimer;

        public FloorQuery(Dictionary<IActorRef, string> actorToSensorId, long requestId, IActorRef requester, TimeSpan timeout)
        {
            this.actorToSensorId = actorToSensorId;
            this.requestId = requestId;
            this.requester = requester;
            this.timeout = timeout;
            stillAwaitingReply = new HashSet<IActorRef>(actorToSensorId.Keys);

            queryTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, QueryTimeout.Instance, Self);

        }

        protected override void PreStart()
        {
            foreach (var sensor in actorToSensorId.Keys)
            {
                Context.Watch(sensor);
                sensor.Tell(new RequestTemperature(TemperatureRequestCorrelationId));
            }
        }

        protected override void PostStop()
        {
            queryTimeoutTimer.Cancel();
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RespondTemperature m when m.RequestId == TemperatureRequestCorrelationId:
                    ITemperatureQueryReading reading;

                    if (m.Temperature.HasValue)
                        reading = new TemperatureAvailable(m.Temperature.Value);
                    else
                        reading = NoTemperatureRecordedYet.Instance;

                    RecordSensorResponse(Sender, reading);
                    break;
                case QueryTimeout m:

                    foreach (var sensor in stillAwaitingReply)
                        repliesReceived.Add(actorToSensorId[sensor], TemperatureSensorTimedOut.Instance);

                    requester.Tell(new RespondAllTemeratures(requestId, repliesReceived.ToImmutableDictionary()));
                    Context.Stop(Self);

                    break;
                case Terminated m:
                    RecordSensorResponse(m.ActorRef, TemperatureSensorNotAvailable.Instance);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        private void RecordSensorResponse(IActorRef sender, ITemperatureQueryReading reading)
        {
            Context.Unwatch(sender);

            stillAwaitingReply.Remove(sender);
            repliesReceived.Add(actorToSensorId[sender], reading);

            if (stillAwaitingReply.Count == 0)
            {
                requester.Tell(new RespondAllTemeratures(requestId, repliesReceived.ToImmutableDictionary()));
                Context.Stop(Self);
            }
        }

        public static Props Prop(Dictionary<IActorRef, string> actorToSensorId, long requestId, IActorRef requester, TimeSpan timeout)
        {
            return Props.Create(() => new FloorQuery(actorToSensorId, requestId, requester, timeout));
        }
    }
}
