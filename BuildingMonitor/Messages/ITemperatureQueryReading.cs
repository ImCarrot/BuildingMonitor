namespace BuildingMonitor.Messages
{
    public interface ITemperatureQueryReading
    {
    }

    public sealed class TemperatureAvailable : ITemperatureQueryReading
    {
        public double Temperature { get; }

        public TemperatureAvailable(double temp)
        {
            Temperature = temp;
        }
    }

    public sealed class NoTemperatureRecordedYet : ITemperatureQueryReading
    {
        public static NoTemperatureRecordedYet Instance { get; } = new NoTemperatureRecordedYet();

        private NoTemperatureRecordedYet() { }
    }

    public sealed class TemperatureSensorNotAvailable : ITemperatureQueryReading
    {
        public static TemperatureSensorNotAvailable Instance { get; } = new TemperatureSensorNotAvailable();

        private TemperatureSensorNotAvailable() { }
    }

    public sealed class TemperatureSensorTimedOut : ITemperatureQueryReading
    {
        public static TemperatureSensorTimedOut Instance { get; } = new TemperatureSensorTimedOut();

        private TemperatureSensorTimedOut() { }
    }
}
