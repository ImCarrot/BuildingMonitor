using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using Xunit;


namespace BuildingMonitor.Tests
{
    public class TempratureSensorShould : TestKit
    {

        [Fact]
        public void InitializeSensorMetaData()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestMetadata(1), probe.Ref);

            var received = probe.ExpectMsg<RespondMetadata>();

            Assert.Equal(1, received.RequestId);
            Assert.Equal("a", received.FloorId);
            Assert.Equal("1", received.SensorId);
        }

        [Fact]
        public void StartWithNoTemperature()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestTemperature(1), probe.Ref);

            var received = probe.ExpectMsg<RespondTemperature>();

            Assert.Equal(1, received.RequestId);
            Assert.Null(received.Temperature);
        }

        [Fact]
        public void ConfirmTemperatureUpdated()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestUpdateTemperature(42, 100), probe.Ref);

            probe.ExpectMsg<RespondTemperatureUpdated>(m =>
            {
                Assert.Equal(42, m.RequestId);
            });


        }

        [Fact]
        public void UpdateTemperature()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestUpdateTemperature(42, 100), probe.Ref);
            sensor.Tell(new RequestTemperature(1), probe.Ref);

            var isUpdated = probe.ExpectMsg<RespondTemperatureUpdated>();

            Assert.Equal(42, isUpdated.RequestId);

            var temperatureReceived = probe.ExpectMsg<RespondTemperature>();

            Assert.Equal(100, temperatureReceived.Temperature);
            Assert.Equal(1, temperatureReceived.RequestId);

        }

        [Fact]
        public void RegisterSensor()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);

            var received = probe.ExpectMsg<RespondSensorRegistered>();

            Assert.Equal(1, received.RequestId);
            Assert.Equal(sensor, received.SensorReference);
        }

        [Fact]
        public void NotRegisterSensorWhenIncorrectFloor()
        {
            var probe = CreateTestProbe();

            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(1, "b", "1"), probe.Ref);

            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
        }

        [Fact]
        public void NotRegisterSensorWhenIncorrectSensorId()
        {
            var probe = CreateTestProbe();

            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

            var sensor = Sys.ActorOf(TemperatureSensor.Prop("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(1, "a", "2"), probe.Ref);

            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
        }
    }
}
