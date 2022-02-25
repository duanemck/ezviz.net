using ezviz.net.domain.deviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    public class Camera : Device
    {
        private readonly EzvizClient client;
        private readonly LoginSession session;

        internal Camera(EzvizDeviceInfo deviceInfo, PagedListResponse response, EzvizClient client) : base(deviceInfo, response)
        {
            this.client = client;
        }

        public string SerialNumber => DeviceInfo.DeviceSerial;
        public string Name => DeviceInfo.Name;
        public string DeviceType => $"{DeviceInfo.DeviceCategory} {DeviceInfo.DeviceSubCategory}";
        public string LocalIp => Wifi.Address ?? Connection.LocalIp ?? "0.0.0.0";
        public AlarmSound AlarmSoundLevel => Status.AlarmSoundMode;

        public async Task<string> GetDetectionSensibilityAsync()
        {

            if (Switches.FirstOrDefault(s => s.Type == SwitchType.AUTO_SLEEP)?.Enable ?? false)
            {
                return "Hibernate";
            }
            else
            {
                var algorithms = await client.GetDetectionSensibility(SerialNumber);
                if (algorithms == null)
                {
                    return "Unknown";
                }
                var type = (DeviceInfo.DeviceCategory == DeviceCategories.BATTERY_CAMERA_DEVICE_CATEGORY)
                    ? "3"
                    : "0";

                return algorithms.FirstOrDefault(alg => alg.Type == type)?.Value ?? "Unknown";
            }

        }

        public async Task SetAlarmSoundLevel(AlarmSound soundLevel, bool enabled)
        {
            await client.SetAlarmSoundLevel(SerialNumber,enabled,soundLevel);
            Status.AlarmSoundMode = soundLevel;
        }
    }
}
