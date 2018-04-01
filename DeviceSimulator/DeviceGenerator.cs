using System.Collections.Generic;

namespace DeviceSimulator
{
    public abstract class DeviceGenerator
    {
        public abstract void Init();
        public abstract List<DeviceMessage> Next();
        public abstract List<DeviceMessage> Next(int seq);
    }
}