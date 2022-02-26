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

        public async Task<ICollection<Alarm>> GetAlarms()
        {
            return await client.GetAlarms(SerialNumber);
        }

        public async Task ToggleSwitch(SwitchType type, bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, type, enabled);
        }

        public async Task ToggleAudio(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.SOUND, enabled);
        }

        public async Task ToggleLed(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.LIGHT, enabled);
        }

        public async Task ToggleInfrared(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.INFRARED_LIGHT, enabled);
        }

        public async Task TogglePrivacyMode(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.PRIVACY, enabled);
        }

        public async Task ToggleSleepMode(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.SLEEP, enabled);
        }

        public async Task ToggleMobileTracking(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.MOBILE_TRACKING, enabled);
        }
    }
}
