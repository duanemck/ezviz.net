using ezviz.net.domain.deviceInfo;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public bool AlarmScheduleEnabled => TimePlans.Any(tp => tp.Type == 2 && tp.Enable == 1);

        public bool UpgradeAvailable => Upgrade.IsNeedUpgrade == 1;
        public bool UpgradeInProgress => Status.UpgradeStatus == 0;
        public decimal UpgradePercent => Status.UpgradeProcess;
        public bool Sleeping => Switches
            .Where(sw => sw.Type == SwitchType.SLEEP || sw.Type == SwitchType.AUTO_SLEEP)
            .Any(sw => sw.Enable);
        public bool PrivacyModeEnabled => Switches.Any(sw => sw.Type == SwitchType.PRIVACY && sw.Enable);
        public bool AudioEnabled => Switches.Any(sw => sw.Type == SwitchType.SOUND && sw.Enable);
        public bool InfraredEnabled => Switches.Any(sw => sw.Type == SwitchType.INFRARED_LIGHT && sw.Enable);
        public bool StateLedEnabled => Switches.Any(sw => sw.Type == SwitchType.LIGHT && sw.Enable);

        public bool MobileTrackingEnabled => Switches.Any(sw => sw.Type == SwitchType.MOBILE_TRACKING && sw.Enable);
        public bool AlarmNotify => Status.GlobalStatus == 1;
        public AlarmSound AlarmSoundMode => Status.AlarmSoundMode;
        public bool IsEncrypted => Status.IsEncrypt == 1;
        public string WANIp => Connection.NetIp;
        public string MacAddress => DeviceInfo.Mac;
        public int LocalRtspPort => Connection.LocalRtspPort;
        public int SupportedChannels => DeviceInfo.ChannelNumber;
        public int BatteryLevel => Status.Optionals.ContainsKey("powerRemaining") ? int.Parse(Status.Optionals["powerRemaining"]) : 0;
        public int PirStatus => Status.PirStatus;
        public async Task<MotionAlarm?> GetLastAlarm()
        {
            var alarms = await GetAlarms();
            var today = DateTime.Now.Date;
            var lastAlarm = alarms.FirstOrDefault();
            if (lastAlarm == null)
            {
                return null;
            }
            var lastAlarmTime = DateTime.ParseExact(
                lastAlarm.AlarmStartTimeStr.Replace("Today",$"{ today:YYYY-MM-DD HH:mm:ss}"), 
                "YYYY-MM-DD HH:mm:ss", CultureInfo.InvariantCulture);

            var timePassedInSeconds = (int)((DateTime.Now - lastAlarmTime).TotalSeconds);

            return new MotionAlarm()
            {
                SecondsSinceLastTrigger = timePassedInSeconds,
                MotionTriggerActive = timePassedInSeconds < 60,
                LastAlarmTime = lastAlarmTime,
                LastAlarmTypeCode = $"{lastAlarm.AlarmType}",
                LastAlarmTypeName = lastAlarm.SampleName ?? "NoAlarm",
                LastAlarmPicture = lastAlarm.PicUrl
            };
        }


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
            await client.SetAlarmSoundLevel(SerialNumber, enabled, soundLevel);
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
