using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace JsonTest
{

    class Program
    {
        public class SensorDataAll
        {
            public SensorDataEntry data { get; set; }
            public SensorDetailMessage detail { get; set; }
        }

        static void Main(string[] args)
        {
            /*
            [sensorData] => [sensorDetail, sensorData] => [BinSensorReading]
             */

            SensorData dataAsArray = GetSensorData(38);

            Console.WriteLine(String.Format("We have {0}  records", dataAsArray.message.lists.Length));

            var source = Observable
                            .Interval(TimeSpan.FromSeconds(1))
                            .Do(x => Console.WriteLine(String.Format("IDX={0}", x)))
                            .Select(idx => dataAsArray.message.lists[idx % dataAsArray.message.lists.Length]);
                            // .Do(x => Console.WriteLine(JsonConvert.SerializeObject(x)));

            source
                .Select(sensorDataEntry => 
                    new SensorDataAll 
                    {
                        detail = GetSensorDetail(sensorDataEntry.sensorallocatedID).message,
                        data = sensorDataEntry
                    })
                // .Do(x => Console.WriteLine(JsonConvert.SerializeObject(x)))
                .Select(_ => 
                            new BinSensorReading
                            {
                                sesnorID = _.detail.sensorsID, // 98,
                                binID = _.detail.currentPinAllocated.projectpinID, // 667,
                                binName = _.detail.currentPinAllocated.name, // "Random Smart Sensor Simulator Module",
                                binCategory = _.detail.currentPinAllocated.pinType.pinTypeName, // "Smart Sensor Simulator ",
                                latitude = _.detail.currentPinAllocated.latitude, // -33.869033,
                                longitude = _.detail.currentPinAllocated.longitude, //151.208895,
                                fillLevel = CalculateFillLevel(_.detail.currentPinAllocated.pinType.depthWhenEmpty_cm,  _.detail.currentPinAllocated.pinType.distanceSensorToFillLine_cm, _.data.ultrasound),
                                temperature = _.data.temperatureValue,
                                timestampdata = _.data.timestampdata
                            })
                // .Do(x => Console.WriteLine(String.Format("FillLevel={0}", x.fillLevel)))
                .Do(x => Console.WriteLine(JsonConvert.SerializeObject(x)))
                .Subscribe(_ => Console.WriteLine($"{DateTime.Now} - Sent message"),
                            ex =>
                            {
                                Console.WriteLine(DateTime.Now + " - Error " + ex.Message);
                                Console.WriteLine(ex.ToString());
                                Console.WriteLine();
                            });
            Console.ReadKey();

            // // Get all sensors
            // SensorList sensors = GetSensorList();

            // if (sensors != null)
            // {
            //     SensorListEntry[] sensorList = sensors.message.lists;
            //     Console.WriteLine(String.Format("There are {0} total sensors", sensorList.Length));
            //     Console.WriteLine(String.Format("There are {0} allocated sensors", sensorList.Where(sensor => sensor.Allocated).Count()));

            //     foreach (SensorListEntry sensor in sensorList)
            //     {
            //         // Only allocated sensors
            //         if (sensor.Allocated)
            //         {
            //             Console.WriteLine(String.Format("Sensor ID {0} named {1} is enabled", sensor.sensorsID, sensor.sensorName));

            //             // Get sensor details
            //             SensorDetail detail = GetSensorDetail(sensor.sensorsID);

            //             if (detail != null)
            //             {
            //                 Console.WriteLine(String.Format("   Sensor ID: {0}", detail.message.sensorsID));
            //                 Console.WriteLine(String.Format("   Bin ID: {0}", detail.message.currentPinAllocated.projectpinID));
            //                 Console.WriteLine(String.Format("   Bin name: {0}", detail.message.currentPinAllocated.name));
            //                 Console.WriteLine(String.Format("   Bin category: {0}", detail.message.currentPinAllocated.pinType.pinTypeName));
            //                 Console.WriteLine(String.Format("   Location.latitude: {0}", detail.message.currentPinAllocated.latitude));
            //                 Console.WriteLine(String.Format("   Location.longitude: {0}", detail.message.currentPinAllocated.longitude));

            //                 // Get sensor data
            //                 SensorData data = GetSensorData(sensor.sensorsID);

            //                 // Start the output JSON file
            //                 File.WriteAllText(@"out/output.json", "[");
            //                 Boolean first = true;
                            
            //                 foreach (SensorDataEntry reading in data.message.lists)
            //                 {
            //                     Console.WriteLine(String.Format("       New reading @ Timestamp: {0}", reading.Timestamp));
            //                     int fillLevel = CalculateFillLevel(data.message.depthWhenEmpty_cm,  data.message.distanceSensorToFillLine_cm, reading.ultrasound);
            //                     Console.WriteLine(String.Format("           Fill level: {0}", fillLevel));
            //                     Console.WriteLine(String.Format("           Temperature: {0}", reading.temperatureValue));

            //                     if (fillLevel < 0)
            //                     {
            //                         Console.WriteLine("> " + data.message.depthWhenEmpty_cm);
            //                         Console.WriteLine("> " + data.message.distanceSensorToFillLine_cm);
            //                         Console.WriteLine("> " + reading.ultrasoundExist);
            //                         Console.WriteLine("> " + reading.ultrasound);
            //                     }

            //                     BinSensorReading message = new BinSensorReading
            //                     {
            //                         sesnorID = detail.message.sensorsID,
            //                         binID = detail.message.currentPinAllocated.projectpinID,
            //                         binName = detail.message.currentPinAllocated.name,
            //                         binCategory = detail.message.currentPinAllocated.pinType.pinTypeName,
            //                         latitude = detail.message.currentPinAllocated.latitude,
            //                         longitude = detail.message.currentPinAllocated.longitude,
            //                         fillLevel = fillLevel,
            //                         temperature = reading.temperatureValue,
            //                         timestampdata = reading.timestampdata
            //                     };

            //                     // Prepend a comma if not first element in output array
            //                     if (!first)
            //                     {
            //                         File.AppendAllText(@"out/output.json", ",");
            //                     }
            //                     // Send message
            //                     File.AppendAllText(@"out/output.json", JsonConvert.SerializeObject(message));
            //                     first = false;
            //                 }

            //                 File.AppendAllText(@"out/output.json", "]");
            //             }
            //         } 
            //     }
            // }
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
