using Akka.Actor;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using System;
using System.Threading.Tasks;

namespace BuildingMonitor.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var system = ActorSystem.Create("buildingMonitoringSystem"))
            {
                IActorRef floorManager = system.ActorOf(FloorsManager.Prop(), "floorManager");

                await CreateSimulatedSensors(floorManager);

                Console.WriteLine("Systems up and running!");
                while (true)
                {
                    var command = Console.ReadKey();

                    if (command.Key == ConsoleKey.Q)
                        Environment.Exit(0);

                    await DisplayTemperatures(system);
                }
            }
        }

        private static async Task DisplayTemperatures(ActorSystem system)
        {
            var temps = await system.ActorSelection("akka://buildingMonitoringSystem/user/floorManager/floor-1st")
                .Ask<RespondAllTemeratures>(new RequestAllTemperatures(0));

            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            foreach (var temp in temps.TemperatureReadings)
            {
                if (temp.Value is TemperatureAvailable value)
                    Console.WriteLine($"Sensor {temp.Key} {temp.Value.GetType().Name} {value.Temperature: 00.0}°C");
                else
                    Console.WriteLine($"Sensor {temp.Key} {temp.Value.GetType().Name}");
            }
        }

        private static async Task CreateSimulatedSensors(IActorRef floorManager)
        {
            for (int i = 0; i < 10; i++)
            {
                var newSimulator = new SimulatedSensor("1st", i.ToString(), floorManager);
                await newSimulator.Connect();

                if (i != 3)
                {
                    newSimulator.StartSendingSimulatedReadings();
                }
            }
        }
    }
}
