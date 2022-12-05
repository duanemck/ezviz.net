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

        public Dictionary<string, JsonElement> Cloud { get; set; } = null!;
        public Dictionary<string, JsonElement> VTM { get; set; } = null!;
        public Dictionary<string, JsonElement> Detector { get; set; } = null!;
        public Dictionary<string, JsonElement> Products_Info { get; set; } = null!;
        public Dictionary<string, JsonElement> P2P { get; set; } = null!;
        public Dictionary<string, JsonElement> Connection { get; set; } = null!;
        public Dictionary<string, JsonElement> KMS { get; set; } = null!;
        public Dictionary<string, JsonElement> Status { get; set; } = null!;
        public Dictionary<string, JsonElement> Time_Plan { get; set; } = null!;
        public Dictionary<string, JsonElement> Channel { get; set; } = null!;
        public Dictionary<string, JsonElement> QOS { get; set; } = null!;
        public Dictionary<string, JsonElement> NoDisturb { get; set; } = null!;
        public Dictionary<string, JsonElement> Feature { get; set; } = null!;
        public Dictionary<string, JsonElement>? Upgrade { get; set; } = null!;
        public Dictionary<string, JsonElement> FeatureInfo { get; set; } = null!;
        public Dictionary<string, JsonElement> Switch { get; set; } = null!;
        public Dictionary<string, JsonElement> CustomTag { get; set; } = null!;
        public Dictionary<string, JsonElement> Video_Quality { get; set; } = null!;
        public Dictionary<string, JsonElement> Wifi { get; set; } = null!;

        public PageInfo Page { get; set; } = null!;
        public IEnumerable<EzvizResourceInfo> ResourceInfos { get; set; } = null!;
        public IEnumerable<EzvizDeviceInfo> DeviceInfos { get; set; } = null!;
    }
}
