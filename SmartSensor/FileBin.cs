using DeviceSimulator;
using IoTAA;

using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SmartSesnor
{
    public class FileBin : DeviceGenerator
    {
        private int counter;
        private int deviceId;
        private string dataFileDir;
        private int binId;
        private string binName;
        private string binCategory;
        private double latitude;
        private double longitude;
        private int depthWhenEmpty;
        private int distanceSensorToFillLine;
        private SensorDataEntry[] data;

        public FileBin(int deviceId, string dataFileDir) 
        {
            this.deviceId = deviceId;
            this.dataFileDir = dataFileDir; // TODO check if dir exists, throw exception
            Init();
        }

        public override void Init()
        {
            this.counter = 0;
            SensorDetailMessage detail = GetSensorDetail().message; // TODO check if file exists, throw exception
            this.binId = detail.currentPinAllocated.projectpinID;
            this.binName = detail.currentPinAllocated.name;
            this.binCategory = detail.currentPinAllocated.pinType.pinTypeName;
            this.latitude = detail.currentPinAllocated.latitude;
            this.longitude = detail.currentPinAllocated.longitude;
            this.depthWhenEmpty = detail.currentPinAllocated.pinType.depthWhenEmpty_cm;
            this.distanceSensorToFillLine = detail.currentPinAllocated.pinType.distanceSensorToFillLine_cm;
            this.data = GetSensorData().message.lists; // TODO check if file exists, throw exception
        }

        public override List<DeviceMessage> Next()
        {
            return new List<DeviceMessage>() {
                new BinSensorReading {
                    sesnorID = this.deviceId,
                    binID = this.binId,
                    binName = this.binName,
                    binCategory = this.binCategory,
                    latitude = this.latitude,
                    longitude = this.longitude,
                    fillLevel = CalculateFillLevel(this.depthWhenEmpty, this.distanceSensorToFillLine, this.data[this.counter % this.data.Length].ultrasound),
                    temperature = this.data[this.counter++ % this.data.Length].temperatureValue,
                    timestampdata = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds())
                }
            };
        }

        public override List<DeviceMessage> Next(int seq)
        {
            return new List<DeviceMessage>() {
                new BinSensorReading {
                    sesnorID = this.deviceId,
                    binID = this.binId,
                    binName = this.binName,
                    binCategory = this.binCategory,
                    latitude = this.latitude,
                    longitude = this.longitude,
                    fillLevel = CalculateFillLevel(this.depthWhenEmpty, this.distanceSensorToFillLine, this.data[seq % this.data.Length].ultrasound),
                    temperature = this.data[seq % this.data.Length].temperatureValue,
                    timestampdata = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds())
                }
            };
        }

        private SensorDetail GetSensorDetail()
        {
            using (StreamReader reader = new StreamReader($"{this.dataFileDir}/sensorDetail.{this.deviceId}.json"))
            {
                return JsonConvert.DeserializeObject<SensorDetail>(reader.ReadToEnd());
            }
        }

        private SensorData GetSensorData()
        {
            using (StreamReader reader = new StreamReader($"{this.dataFileDir}/sensorData.{this.deviceId}.json"))
            {
                return JsonConvert.DeserializeObject<SensorData>(reader.ReadToEnd());
            }
        }

        private int CalculateFillLevel(int depthWhenEmpty, int distanceToFillLine, int ultrasound)
        {
            if (ultrasound == depthWhenEmpty + distanceToFillLine)
            {
                return 0;
            }
            else 
            {
                return Convert.ToInt32(
                    Math.Round(
                        Convert.ToDecimal(depthWhenEmpty + distanceToFillLine - ultrasound)
                        /
                        Convert.ToDecimal(depthWhenEmpty) 
                        * 
                        100)
                    );
            }
        }
    }
}