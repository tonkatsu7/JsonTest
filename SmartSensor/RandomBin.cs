using DeviceSimulator;
using IoTAA;

using System;
using System.Collections.Generic;

namespace SmartSesnor
{
    public class RandomBin : DeviceGenerator
    {
        private Random rand;
        private int deviceId;

        public RandomBin(int binId) 
        {
            this.deviceId = binId;
            Init();
        }

        public override void Init()
        {
            this.rand = new Random();
        }

        public override List<DeviceMessage> Next()
        {
            return new List<DeviceMessage>() {
                new BinSensorReading {
                    sesnorID = this.deviceId,
                    binID = 667,
                    binName = "Random Smart Sensor Simulator Module",
                    binCategory = "Smart Sensor Simulator ",
                    latitude = -33.869033,
                    longitude = 151.208895,
                    fillLevel = this.rand.Next(0,101),
                    temperature = this.rand.Next(10, 36),
                    timestampdata = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds())
                }
            };
        }

        public override List<DeviceMessage> Next(int seq)
        {
            return Next();
        }
    }
}