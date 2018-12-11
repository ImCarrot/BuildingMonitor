using Akka.Actor;
using BuildingMonitor.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingMonitor.Host
{
    public class SimulatedSensor
    {
        private IActorRef floorManager;
        private readonly Random temperatureGenerator;
        private readonly string floorId;
        private readonly string sensorId;
        private IActorRef sensorRef;
        private Timer timer;

        public SimulatedSensor(string floorId, string sensorId, IActorRef floorManager)
        {
            this.floorManager = floorManager;
            this.floorId = floorId;
            this.sensorId = sensorId;
            temperatureGenerator = new Random(int.Parse(sensorId));
        }

        public async Task Connect()
        {
            var response = await floorManager.Ask<RespondSensorRegistered>(
                new RequestRegisterTemperatureSensor(1, floorId, sensorId));

            sensorRef = response.SensorReference;

        }

        public void StartSendingSimulatedReadings()
        {
            timer = new Timer(SimulateUpdateTemperature, null, 0, 1000);
        }

        private void SimulateUpdateTemperature(object state)
        {
            var randomTemerature = temperatureGenerator.NextDouble();
            randomTemerature *= 10;
            sensorRef.Ask(new RequestUpdateTemperature(0, randomTemerature));
        }
    }
}
