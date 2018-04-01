using DeviceSimulator;
using IoTAA;

using System;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace SmartSesnor
{
    public class ApiBin : DeviceGenerator
    {
        public enum TimeInterval 
        {
            HOURLY      = 3600, 
            FOUR_HOURLY = 14400, 
            DAILY       = 86400, 
            WEEKLY      = 604800
        };
        private DateTimeOffset fromOrigin;
        private int counter;
        private TimeInterval periodicity;
        private int deviceId;
        private int binId;
        private string binName;
        private string binCategory;
        private double latitude;
        private double longitude;
        private int depthWhenEmpty;
        private int distanceSensorToFillLine;

        public ApiBin(int deviceId, TimeInterval periodicity, DateTimeOffset? fromOrigin = null) 
        {
            this.deviceId = deviceId;
            this.periodicity = periodicity;
            if (!fromOrigin.HasValue)
            {
                this.fromOrigin = DateTimeOffset.FromUnixTimeSeconds(1500595200); // TODO temp equiv to 2017-07-21T00:00:00Z 
            }
            Init();
        }

        public override void Init()
        {
            this.counter = 0;
            SensorDetailMessage detail = GetSensorDetail().message;
            this.binId = detail.currentPinAllocated.projectpinID;
            this.binName = detail.currentPinAllocated.name;
            this.binCategory = detail.currentPinAllocated.pinType.pinTypeName;
            this.latitude = detail.currentPinAllocated.latitude;
            this.longitude = detail.currentPinAllocated.longitude;
            this.depthWhenEmpty = detail.currentPinAllocated.pinType.depthWhenEmpty_cm;
            this.distanceSensorToFillLine = detail.currentPinAllocated.pinType.distanceSensorToFillLine_cm;
        }

        public override List<DeviceMessage> Next() 
        {
            DateTimeOffset queryFrom = this.fromOrigin.Add(new TimeSpan(0, 0, this.counter++ * (int)this.periodicity));
            List<SensorDataEntry> fetchResult = 
                FetchSensorDataAsync(queryFrom, new TimeSpan(0, 0, (int)this.periodicity)).GetAwaiter().GetResult(); // just block to wait for async method
            IEnumerable<DeviceMessage> enums =
                fetchResult.Select(_ => new BinSensorReading   
                    {
                        sesnorID = this.deviceId,
                        binID = this.binId,
                        binName = this.binName,
                        binCategory = this.binCategory,
                        latitude = this.latitude,
                        longitude = this.longitude,
                        fillLevel = CalculateFillLevel(this.depthWhenEmpty, this.distanceSensorToFillLine, _.ultrasound),
                        temperature = _.temperatureValue,
                        timestampdata = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds())
                    });
            return enums.ToList();
        }
        
        public override List<DeviceMessage> Next(int seq) 
        {
            return Next();
        }

        private SensorDetail GetSensorDetail() // TODO TEMP should call API
        {
            using (StreamReader reader = new StreamReader($"data/sensorDetail.{this.deviceId}.json"))
            {
                return JsonConvert.DeserializeObject<SensorDetail>(reader.ReadToEnd());
            }
        }

        private async Task<List<SensorDataEntry>> FetchSensorDataAsync(DateTimeOffset queryFrom, TimeSpan duration)
        {
            long fromUnixTime = queryFrom.ToUnixTimeSeconds();
            long toUnixTime = fromUnixTime + Convert.ToInt64(duration.TotalSeconds);

            string baseUrl = "https://dashboard.smartsensor.com.au/";
            string rawSensorDataUrl = $"api/sensors/rawsensordata/{this.deviceId}?dmin={fromUnixTime}&dmax={toUnixTime}";

            using (var client = new HttpClient())
            {
                string url = baseUrl + rawSensorDataUrl;
                var body = new
                {
                    deviceID = "ABCD-EFGH-IJKL-MNOP",
                    token = "c3eeaab10e3c93f6c9da95027c14a283"
                };
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
                // response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var rootJson = JsonConvert.DeserializeObject<SensorData>(result);
                    var arr = rootJson.message.lists;
                    var lst = arr.OfType<SensorDataEntry>().ToList();
                    // Console.WriteLine($"API call returned {lst.Count} entries");
                    return lst;
                }
                else
                {
                    return new List<SensorDataEntry>();
                }
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