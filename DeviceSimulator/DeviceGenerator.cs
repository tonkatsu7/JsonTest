namespace DeviceSimulator
{
    public abstract class DeviceGenerator
    {
        public abstract DeviceMessage Next();
        public abstract DeviceMessage Next(int seq);
    }
}