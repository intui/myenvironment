using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Sensors.OneWire.Common
{
    class DataItem
    {
        int ID { get; set; }
        Guid sensorID { get; set; }
        public DateTime captureTime { get; set; }
        double temperature { get; set; }
        double humidity { get; set; }
        public DataItem(int ID, Guid sensorID, DateTime captureTime, float temperature, float humidity)
        {
            this.ID = ID;
            this.sensorID = sensorID;
            this.captureTime = captureTime;
            this.temperature = Math.Round(temperature, 1);
            this.humidity = Math.Round(humidity, 1);
        }
        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject["ID"] = JsonValue.CreateStringValue(ID.ToString());
            jsonObject["sensorID"] = JsonValue.CreateStringValue(sensorID.ToString());
            jsonObject["captureTime"] = JsonValue.CreateStringValue(captureTime.ToUniversalTime().ToString()); // UTC or ticks?
            jsonObject["temperature"] = JsonValue.CreateNumberValue(temperature);
            jsonObject["humidity"] = JsonValue.CreateNumberValue(humidity);
            return jsonObject.Stringify();
        }
        public JsonObject ToJsonObject()
        {
            JsonObject itemObject = new JsonObject();
            itemObject.SetNamedValue("ID", JsonValue.CreateStringValue(ID.ToString()));
            itemObject.SetNamedValue("sensorID", JsonValue.CreateStringValue(sensorID.ToString()));
            itemObject.SetNamedValue("captureTime", JsonValue.CreateStringValue(captureTime.ToUniversalTime().ToString()));
            itemObject.SetNamedValue("temperature", JsonValue.CreateNumberValue(Math.Round(temperature,1)));
            itemObject.SetNamedValue("humidity", JsonValue.CreateNumberValue(Math.Round(humidity,1)));
            return itemObject;
        }
    }
}
