using ezviz.net.domain.deviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    internal class PagedListResponse : GenericResponse
    { 

        public Dictionary<string, JsonElement> Cloud { get; set; }
        public Dictionary<string, JsonElement> VTM { get; set; }
        public Dictionary<string, JsonElement> Detector { get; set; }
        public Dictionary<string, JsonElement> Products_Info { get; set; }
        public Dictionary<string, JsonElement> P2P { get; set; }
        public Dictionary<string, JsonElement> Connection { get; set; }
        public Dictionary<string, JsonElement> KMS { get; set; }
        public Dictionary<string, JsonElement> Status { get; set; }
        public Dictionary<string, JsonElement> Time_Plan { get; set; }
        public Dictionary<string, JsonElement> Channel { get; set; }
        public Dictionary<string, JsonElement> QOS { get; set; }
        public Dictionary<string, JsonElement> NoDisturb { get; set; }
        public Dictionary<string, JsonElement> Feature { get; set; }
        public Dictionary<string, JsonElement> Upgrade { get; set; }
        public Dictionary<string, JsonElement> FeatureInfo { get; set; }
        public Dictionary<string, JsonElement> Switch { get; set; }
        public Dictionary<string, JsonElement> CustomTag { get; set; }
        public Dictionary<string, JsonElement> Video_Quality { get; set; }
        public Dictionary<string, JsonElement> Wifi { get; set; }

        public PageInfo Page { get; set; }
        public IEnumerable<EzvizResourceInfo> ResourceInfos { get; set; }
        public IEnumerable<EzvizDeviceInfo> DeviceInfos { get; set; }
    }
}
