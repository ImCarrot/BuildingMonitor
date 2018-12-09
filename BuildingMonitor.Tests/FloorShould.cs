using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using Xunit;


namespace BuildingMonitor.Tests
{
    public class FloorShould : TestKit
    {
        [Fact]
        public void RegisterNewSensorIfDoesntExist()
        {
            var probe = CreateTestProbe();

            var floor = Sys.ActorOf(Floor.Prop("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);

            var received = probe.ExpectMsg<RespondSensorRegistered>();
            Assert.Equal(1, received.RequestId);

            var sensor = probe.LastSender;

            sensor.Tell(new RequestUpdateTemperature(42, 100), probe.Ref);
            
            probe.ExpectMsg<RespondTemperatureUpdated>();
            
        }

        [Fact]
        public void ReturnExistingSensorWhenRegisteringSameSensor()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Prop("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
            var received = probe.ExpectMsg<RespondSensorRegistered>();
            Assert.Equal(1, received.RequestId);
            var firstSensor = probe.LastSender;

            floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "42"), probe.Ref);
            received = probe.ExpectMsg<RespondSensorRegistered>();
            Assert.Equal(2, received.RequestId);
            var secondSensor = probe.LastSender;

            Assert.Equal(firstSensor, secondSensor);
        }

        [Fact]
        public void NotRegisterWhenMismatchedFloor()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

            var floor = Sys.ActorOf(Floor.Prop("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1, "b", "42"), probe.Ref);
            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
            Assert.Equal(floor, unhandled.Recipient);
        }

        [Fact]
        public void ReturnAllSensorIds()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Prop("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();

            floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "80"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();

            floor.Tell(new RequestTemperatureSensorIds(1), probe.Ref);
            var response = probe.ExpectMsg<RespondTempratureSenssorIds>();

            Assert.Equal(2, response.Ids.Count);
            Assert.Contains("42", response.Ids);
            Assert.Contains("80", response.Ids);
        }

        [Fact]
        public void ReturnEmptyIfNotAdded()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Prop("a"));

            floor.Tell(new RequestTemperatureSensorIds(1), probe.Ref);
            var response = probe.ExpectMsg<RespondTempratureSenssorIds>();

            Assert.Equal(0, response.Ids.Count);
        }

        [Fact]
        public void ReturnOnlyActiveSensors()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Prop("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();

            var firstSensor = probe.LastSender;

            floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "80"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();

            probe.Watch(firstSensor);

            firstSensor.Tell(PoisonPill.Instance);

            probe.ExpectTerminated(firstSensor);

            floor.Tell(new RequestTemperatureSensorIds(3), probe.Ref);
            var response = probe.ExpectMsg<RespondTempratureSenssorIds>();

            Assert.Equal(1, response.Ids.Count);
            Assert.Contains("80", response.Ids);
        }
    }
}
