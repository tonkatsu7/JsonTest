using DeviceSimulator;
using IoTAA;
using SmartSesnor;

using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace JsonTest
{
    class Program
    {
        private enum DataSource {Random, File, API};

        static void Main(string[] args)
        {
            int deviceId = 38;

            DataSource mode = DataSource.API;

            /*
            [sensorData] => [sensorDetail, sensorData] => [BinSensorReading]
             */

            // SensorData dataAsArray = GetSensorData(38);
            // Console.WriteLine(String.Format("We have {0}  records", dataAsArray.message.lists.Length));

            // int interval = 86400;

            DeviceGenerator device;
            
            switch (mode)
            {
                case DataSource.File:
                    device = new FileBin(deviceId, "data");
                    break;
                case DataSource.API:
                    device = new ApiBin(deviceId, ApiBin.TimeInterval.FOUR_HOURLY);
                    break;
                case DataSource.Random:
                default:
                    device = new RandomBin(deviceId);
                    break;
            }

            var source = Observable
                            .Interval(TimeSpan.FromSeconds(2))
                            .Do(x => Console.WriteLine(String.Format("IDX={0}", x)))
                            // .Select(idx => dataAsArray.message.lists[idx % dataAsArray.message.lists.Length]);
                            // .SelectMany(async idx => 
                            // {
                            //     return await FetchSensorDataAsync(38, DateTimeOffset.FromUnixTimeSeconds(1500613837 + idx * interval), new TimeSpan(0, 0, interval));
                            // }); // flatten from Task<List<SensorDataEntry> to List<SensorDataEntry>
                            .Select(_ => device.Next())
                            // .Do(x => Console.WriteLine(JsonConvert.SerializeObject(x)))
                            ;

            // IEnumerable<SensorDataEntry> enums = dataAsArray.message.lists;
            // var source2 = enums.ToObservable();

            source
                .SelectMany(_ => _) // flatten from List<SensorDataEntry> to SensorDataEntry
            //     .Select(_ => 
            //                 new
            //                 {
            //                     detail = GetSensorDetail(_.sensorallocatedID).message,
            //                     data = _
            //                 })
            //     // .Do(x => Console.WriteLine(JsonConvert.SerializeObject(x)))
            //     .Select(_ => 
            //                 new BinSensorReading
            //                 {
            //                     sesnorID = _.detail.sensorsID, // 98,
            //                     binID = _.detail.currentPinAllocated.projectpinID, // 667,
            //                     binName = _.detail.currentPinAllocated.name, // "Random Smart Sensor Simulator Module",
            //                     binCategory = _.detail.currentPinAllocated.pinType.pinTypeName, // "Smart Sensor Simulator ",
            //                     latitude = _.detail.currentPinAllocated.latitude, // -33.869033,
            //                     longitude = _.detail.currentPinAllocated.longitude, //151.208895,
            //                     fillLevel = CalculateFillLevel(_.detail.currentPinAllocated.pinType.depthWhenEmpty_cm,  _.detail.currentPinAllocated.pinType.distanceSensorToFillLine_cm, _.data.ultrasound),
            //                     temperature = _.data.temperatureValue,
            //                     timestampdata = _.data.timestampdata
            //                 })
            //     // .Do(x => Console.WriteLine(String.Format("FillLevel={0}", x.fillLevel)))
                .Do(x => Console.WriteLine(JsonConvert.SerializeObject(x)))
                .Subscribe(_ => Console.WriteLine($"{DateTimeOffset.Now} - Sent message"),
                            ex =>
                                {
                                    Console.WriteLine(DateTimeOffset.Now + " - Error " + ex.Message);
                                    Console.WriteLine(ex.ToString());
                                    Console.WriteLine();
                                });
            Console.ReadKey();
        }

        private static SensorList GetSensorList()
        {
            using (StreamReader reader = new StreamReader("data/sensorList.json"))
            {
                return JsonConvert.DeserializeObject<SensorList>(reader.ReadToEnd());
            }
        }

        // private static SensorDetail GetSensorDetail(int sensorId)
        // {
        //     using (StreamReader reader = new StreamReader(String.Format("data/sensorDetail.{0}.json", sensorId)))
        //     {
        //         return JsonConvert.DeserializeObject<SensorDetail>(reader.ReadToEnd());
        //     }
        // }

        // private static SensorData GetSensorData(int sensorId)
        // {
        //     using (StreamReader reader = new StreamReader(String.Format("data/sensorData.{0}.json", sensorId)))
        //     {
        //         return JsonConvert.DeserializeObject<SensorData>(reader.ReadToEnd());
        //     }
        // }

        // private static async Task<List<SensorDataEntry>> FetchSensorDataAsync(int sensorId, DateTimeOffset from, TimeSpan duration)
        // {
        //     long fromUnixTime = from.ToUnixTimeSeconds();
        //     long toUnixTime = fromUnixTime + Convert.ToInt64(duration.TotalSeconds);

        //     string baseUrl = "https://dashboard.smartsensor.com.au/";
        //     string rawSensorDataUrl = $"api/sensors/rawsensordata/{sensorId}?dmin={fromUnixTime}&dmax={toUnixTime}";

        //     using (var client = new HttpClient())
        //     {
        //         string url = baseUrl + rawSensorDataUrl;
        //         var body = new
        //         {
        //             deviceID = "ABCD-EFGH-IJKL-MNOP",
        //             token = "544dfc3d64f4c9d0e60c91fbc0710ef8"
        //         };
        //         HttpResponseMessage response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
        //         // response.EnsureSuccessStatusCode();
        //         if (response.IsSuccessStatusCode)
        //         {
        //             string result = await response.Content.ReadAsStringAsync();
        //             var rootJson = JsonConvert.DeserializeObject<SensorData>(result);
        //             var arr = rootJson.message.lists;
        //             var lst = arr.OfType<SensorDataEntry>().ToList();
        //             // Console.WriteLine($"API call returned {lst.Count} entries");
        //             return lst;
        //         }
        //         else
        //         {
        //             return new List<SensorDataEntry>();
        //         }
        //     }
        // }

        // private static int CalculateFillLevel(int depthWhenEmpty, int distanceToFillLine, int ultrasound)
        // {
        //     if (ultrasound == depthWhenEmpty + distanceToFillLine)
        //     {
        //         return 0;
        //     }
        //     else 
        //     {
        //         return Convert.ToInt32(
        //             Math.Round(
        //                 Convert.ToDecimal(depthWhenEmpty + distanceToFillLine - ultrasound)
        //                 /
        //                 Convert.ToDecimal(depthWhenEmpty) 
        //                 * 
        //                 100)
        //             );
        //     }
        // }
    }
}
