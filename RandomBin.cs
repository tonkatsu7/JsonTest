using SmartSesnor;
using DeviceSimulator;
using IoTAA;

using System;

namespace JsonTest
{
    public class RandomBin : DeviceGenerator
    {
        private Random rand;
        // private int counter;
        private int deviceId;

        public RandomBin(int binId) 
        {
            this.rand = new Random();
            // this.counter = 0;
            this.deviceId = binId;
        }

        public override DeviceMessage Next()
        {
            return new BinSensorReading {
                sesnorID = this.deviceId, //_.detail.sensorsID, // 98,
                binID = 667, //_.detail.currentPinAllocated.projectpinID, // 667,
                binName = "Random Smart Sensor Simulator Module", //_.detail.currentPinAllocated.name, // "Random Smart Sensor Simulator Module",
                binCategory = "Smart Sensor Simulator ", //_.detail.currentPinAllocated.pinType.pinTypeName, // "Smart Sensor Simulator ",
                latitude = -33.869033, //_.detail.currentPinAllocated.latitude, // -33.869033,
                longitude = 151.208895, //_.detail.currentPinAllocated.longitude, // 151.208895,
                fillLevel = this.rand.Next(0,101), //CalculateFillLevel(_.detail.currentPinAllocated.pinType.depthWhenEmpty_cm,  _.detail.currentPinAllocated.pinType.distanceSensorToFillLine_cm, _.data.ultrasound),
                temperature = this.rand.Next(10, 36), //_.data.temperatureValue,
                timestampdata = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds())
            };
            // return new SensorDataEntry {
            //     sensorsdataID = this.counter++, //23660,
            //     sensorstokenID = 1,
            //     sensorallocatedID = this.deviceId,
            //     sensorEventID = "00000000003b045e",
            //     sensorDeviceID = "B6060101C0000543",
            //     firmwareVersion = "1.2.2.0",
            //     headerMethod = "heartbeat",
            //     reason = "SAMPLE_PERIOD",
            //     temperatureExist = "Y",
            //     temperatureValue = this.rand.Next(10, 36),
            //     temperatureOkay = "Y",
            //     accelerometer_x = 0.0498046875,
            //     accelerometer_y = -0.03564453125,
            //     accelerometer_z = 1.00439453125,
            //     ultrasoundExist = "Y",
            //     ultrasound = this.rand.Next(10, 101),
            //     batteryVoltage_mV = 3482,
            //     signalStrengthExist = "Y",
            //     signalStrength_rssi_dbm = -61,
            //     signalStrength_bitErrorRate = 25,
            //     timestampdata = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds()) //1500613837
            // };
        }

        public override DeviceMessage Next(int seq)
        {
            return Next();
        }
    }
}