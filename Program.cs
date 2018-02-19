using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace JsonTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get all sensors
            SensorList sensors = GetSensorList();

            if (sensors != null)
            {
                SensorListEntry[] sensorList = sensors.message.lists;
                Console.WriteLine(String.Format("There are {0} total sensors", sensorList.Length));
                Console.WriteLine(String.Format("There are {0} allocated sensors", sensorList.Where(sensor => sensor.Allocated).Count()));

                foreach (SensorListEntry sensor in sensorList)
                {
                    // Only allocated sensors
                    if (sensor.Allocated)
                    {
                        Console.WriteLine(String.Format("Sensor ID {0} named {1} is enabled", sensor.sensorsID, sensor.sensorName));

                        // Get sensor details
                        SensorDetail detail = GetSensorDetail(sensor.sensorsID);

                        if (detail != null)
                        {
                            Console.WriteLine(String.Format("   Sensor ID: {0}", detail.message.sensorsID));
                            Console.WriteLine(String.Format("   Bin ID: {0}", detail.message.currentPinAllocated.projectpinID));
                            Console.WriteLine(String.Format("   Bin name: {0}", detail.message.currentPinAllocated.name));
                            Console.WriteLine(String.Format("   Bin category: {0}", detail.message.currentPinAllocated.pinType.pinTypeName));
                            Console.WriteLine(String.Format("   Location.latitude: {0}", detail.message.currentPinAllocated.latitude));
                            Console.WriteLine(String.Format("   Location.longitude: {0}", detail.message.currentPinAllocated.longitude));

                            // Get sensor data
                            SensorData data = GetSensorData(sensor.sensorsID);
                            
                            foreach (SensorDataEntry reading in data.message.lists)
                            {
                                Console.WriteLine(String.Format("       New reading @ Timestamp: {0}", reading.Timestamp));
                                int fillLevel = CalculateFillLevel(data.message.depthWhenEmpty_cm,  data.message.distanceSensorToFillLine_cm, reading.ultrasound);
                                Console.WriteLine(String.Format("           Fill level: {0}", fillLevel));
                                Console.WriteLine(String.Format("           Temperature: {0}", reading.temperatureValue));
                            }
                        }
                    } 
                }
            }
        }

        private static SensorList GetSensorList()
        {
            using (StreamReader reader = new StreamReader("data/sensorList.json"))
            {
                return JsonConvert.DeserializeObject<SensorList>(reader.ReadToEnd());
            }
        }

        private static SensorDetail GetSensorDetail(int sensorId)
        {
            using (StreamReader reader = new StreamReader(String.Format("data/sensorDetail.{0}.json", sensorId)))
            {
                return JsonConvert.DeserializeObject<SensorDetail>(reader.ReadToEnd());
            }
        }

        private static SensorData GetSensorData(int sensorId)
        {
            using (StreamReader reader = new StreamReader(String.Format("data/sensorData.{0}.json", sensorId)))
            {
                return JsonConvert.DeserializeObject<SensorData>(reader.ReadToEnd());
            }
        }

        private static int CalculateFillLevel(int depthWhenEmpty, int distanceToFillLine, int ultrasound)
        {
            // int fillLevelInCMs = depthWhenEmpty + distanceToFillLine - ultrasound;
            // decimal fillLevelPercentage = Convert.ToDecimal(fillLevelInCMs)/Convert.ToDecimal(depthWhenEmpty) * 100;
            // return Convert.ToInt32(Math.Round(fillLevelPercentage));
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
