using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using System;
using System.Collections.Generic;
using Xunit;

namespace BuildingMonitor.Tests
{
    public class FloorQueryShould : TestKit
    {
        [Fact]
        public void RequestTemperatures()
        {
            var queryRequester = CreateTestProbe();
            var sensor1 = CreateTestProbe();
            var sensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Prop(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [sensor1.Ref] = "sensor1",
                    [sensor2.Ref] = "sensor2",
                },
                requestId: 1,
                requester: queryRequester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            sensor1.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(sender, floorQuery);
            });

            sensor2.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(sender, floorQuery);
            });

            floorQuery.Tell(new RespondTemperature(FloorQuery.TemperatureRequestCorrelationId, 23.9), sensor1.Ref);
            floorQuery.Tell(new RespondTemperature(FloorQuery.TemperatureRequestCorrelationId, 32.4), sensor2.Ref);

            var response = queryRequester.ExpectMsg<RespondAllTemeratures>();

            Assert.Equal(1, response.RequestId);
            Assert.Equal(2, response.TemperatureReadings.Count);

            var reading1 = Assert.IsAssignableFrom<TemperatureAvailable>(response.TemperatureReadings["sensor1"]);

            Assert.Equal(23.9, reading1.Temperature);

            var reading2 = Assert.IsAssignableFrom<TemperatureAvailable>(response.TemperatureReadings["sensor2"]);

            Assert.Equal(32.4, reading2.Temperature);
        }

        [Fact]
        public void ReturnNoTemperatureAvailableResults()
        {
            var queryRequester = CreateTestProbe();
            var sensor1 = CreateTestProbe();
            var sensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Prop(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [sensor1.Ref] = "sensor1",
                    [sensor2.Ref] = "sensor2",
                },
                requestId: 1,
                requester: queryRequester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));
            
            sensor1.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(sender, floorQuery);
            });

            sensor2.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(sender, floorQuery);
            });

            floorQuery.Tell(new RespondTemperature(FloorQuery.TemperatureRequestCorrelationId, null), sensor1.Ref);
            floorQuery.Tell(new RespondTemperature(FloorQuery.TemperatureRequestCorrelationId, 32.4), sensor2.Ref);

            var response = queryRequester.ExpectMsg<RespondAllTemeratures>();

            Assert.Equal(1, response.RequestId);
            Assert.Equal(2, response.TemperatureReadings.Count);

            Assert.IsAssignableFrom<NoTemperatureRecordedYet>(response.TemperatureReadings["sensor1"]);

            var reading2 = Assert.IsAssignableFrom<TemperatureAvailable>(response.TemperatureReadings["sensor2"]);

            Assert.Equal(32.4, reading2.Temperature);
        }

        [Fact]
        public void RecognizeSensorsThatStoppedDuringQuery()
        {
            var queryRequester = CreateTestProbe();
            var sensor1 = CreateTestProbe();
            var sensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Prop(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [sensor1.Ref] = "sensor1",
                    [sensor2.Ref] = "sensor2",
                },
                requestId: 1,
                requester: queryRequester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            sensor1.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });

            sensor2.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });

            floorQuery.Tell(new RespondTemperature(FloorQuery.TemperatureRequestCorrelationId, 23.9), sensor1.Ref);
            sensor2.Tell(PoisonPill.Instance);

            var response = queryRequester.ExpectMsg<RespondAllTemeratures>();

            Assert.Equal(1, response.RequestId);
            Assert.Equal(2, response.TemperatureReadings.Count);

            var reading1 = Assert.IsAssignableFrom<TemperatureAvailable>(response.TemperatureReadings["sensor1"]);

            Assert.Equal(23.9, reading1.Temperature);

            Assert.IsAssignableFrom<TemperatureSensorNotAvailable>(response.TemperatureReadings["sensor2"]);
        }

        [Fact]
        public void TimeoutWhenSomeSensorsDoNotReturnTimelyResponse()
        {
            var queryRequester = CreateTestProbe();
            var sensor1 = CreateTestProbe();
            var sensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Prop(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [sensor1.Ref] = "sensor1",
                    [sensor2.Ref] = "sensor2",
                },
                requestId: 1,
                requester: queryRequester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            sensor1.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(sender, floorQuery);
            });

            sensor2.ExpectMsg<RequestTemperature>((m, sender) =>
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(sender, floorQuery);
            });

            floorQuery.Tell(new RespondTemperature(FloorQuery.TemperatureRequestCorrelationId, 23.9), sensor1.Ref);
            // not responding from sensor 2, so that the request times out. :))

            var response = queryRequester.ExpectMsg<RespondAllTemeratures>(TimeSpan.FromSeconds(5));

            Assert.Equal(1, response.RequestId);
            Assert.Equal(2, response.TemperatureReadings.Count);

            var reading1 = Assert.IsAssignableFrom<TemperatureAvailable>(response.TemperatureReadings["sensor1"]);

            Assert.Equal(23.9, reading1.Temperature);

            Assert.IsAssignableFrom<TemperatureSensorTimedOut>(response.TemperatureReadings["sensor2"]);
        }
    }
}
