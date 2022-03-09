using ezviz.net.domain.deviceInfo;
using ezviz.net.util;
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

        public string? SerialNumber => DeviceInfo?.DeviceSerial;
        public string? Name => DeviceInfo?.Name;
        public string? DeviceType => $"{DeviceInfo?.DeviceCategory} {DeviceInfo?.DeviceSubCategory}";
        public string? LocalIp => Wifi?.Address ?? Connection?.LocalIp ?? "0.0.0.0";
        public AlarmSound? AlarmSoundLevel => Status?.AlarmSoundMode;
        public bool AlarmScheduleEnabled => TimePlans?.Any(tp => tp.Type == 2 && tp.Enable == 1) ?? false;

        public bool UpgradeAvailable => Upgrade?.IsNeedUpgrade == 1;
        public bool UpgradeInProgress => Status?.UpgradeStatus == 0;
        public decimal UpgradePercent => Status?.UpgradeProcess ?? 0M;
        public bool Sleeping => Switches?
            .Where(sw => sw.Type == SwitchType.Sleep || sw.Type == SwitchType.AutoSleep)
            .Any(sw => sw.Enable) ?? false;
        public bool PrivacyModeEnabled => Switches?.Any(sw => sw.Type == SwitchType.Privacy && sw.Enable) ?? false;
        public bool AudioEnabled => Switches?.Any(sw => sw.Type == SwitchType.Sound && sw.Enable) ?? false;
        public bool InfraredEnabled => Switches?.Any(sw => sw.Type == SwitchType.InfraredLight && sw.Enable) ?? false;
        public bool StateLedEnabled => Switches?.Any(sw => sw.Type == SwitchType.Light && sw.Enable) ?? false;

        public bool MobileTrackingEnabled => Switches?.Any(sw => sw.Type == SwitchType.MobileTracking && sw.Enable) ?? false;
        public bool AlarmNotify => Status?.GlobalStatus == 1;
        public bool NotifyOffline => DeviceInfo?.OfflineNotify == 1;
        public AlarmSound? AlarmSoundMode => Status?.AlarmSoundMode;
        public bool IsEncrypted => Status?.IsEncrypt == 1;
        public string? WANIp => Connection?.NetIp;
        public string? MacAddress => DeviceInfo?.Mac;
        public int? LocalRtspPort => Connection?.LocalRtspPort;
        public int? SupportedChannels => DeviceInfo?.ChannelNumber;
        public int? BatteryLevel => (Status != null && Status.Optionals.ContainsKey("powerRemaining")) ? int.Parse(Status.Optionals["powerRemaining"]) : 0;
        public int PirStatus => Status?.PirStatus ?? 0;

        public DetectionSensitivityLevel DetectionSensitivity { get; set; }
        public bool? Online => Status != null && Status.Optionals.ContainsKey("OnlineStatus") ? Status.Optionals["OnlineStatus"] == "1" : null;
        public decimal DiskCapacityMB => Status != null && Status.Optionals.ContainsKey("diskCapacity")
            ? decimal.Parse(Status.Optionals["diskCapacity"].Split(",").First())
            : 0M;

        public decimal DiskCapacityGB => DiskCapacityMB / 1024;

        public AlarmDetectionMethod AlarmDetectionMethod { get; set; }

        public async Task<MotionAlarm?> GetLastAlarm()
        {
            var alarms = await GetAlarms();
            var today = DateTime.Now.Date;
            var lastAlarm = alarms.FirstOrDefault();
            if (lastAlarm == null)
            {
                return null;
            }
            var format = "yyyy-MM-dd HH:mm:ss";
            var lastAlarmTime = DateTime.ParseExact(
                lastAlarm.AlarmStartTimeStr.Replace("Today", today.ToString(format)),
                format, CultureInfo.InvariantCulture);

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


        public async Task<DetectionSensitivityLevel> GetDetectionSensibilityAsync()
        {
            DetectionSensitivity = DetectionSensitivityLevel.Unknown;
            if (Switches?.FirstOrDefault(s => s.Type == SwitchType.AutoSleep)?.Enable ?? false)
            {
                DetectionSensitivity = DetectionSensitivityLevel.Hibernate;
            }
            else
            {
                var algorithms = await client.GetDetectionSensibility(SerialNumber);
                if (algorithms != null)
                {
                    var type = (DeviceInfo?.DeviceCategory == DeviceCategories.BATTERY_CAMERA_DEVICE_CATEGORY)
                        ? "3"
                        : "0";
                    var algorithmValue = algorithms.FirstOrDefault(alg => alg.Type == type)?.Value;
                    if (algorithmValue != null)
                    {
                        DetectionSensitivity = EnumX.ToObject<DetectionSensitivityLevel>(int.Parse(algorithmValue));
                    }
                }
            }
            return DetectionSensitivity;
        }

        public async Task Arm()
        {
            await client.SetCameraArmed(SerialNumber, true);
        }

        public async Task Disarm()
        {
            await client.SetCameraArmed(SerialNumber, false);
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
            await client.ChangeSwitch(SerialNumber, SwitchType.Sound, enabled);
        }

        public async Task ToggleLed(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Light, enabled);
        }

        public async Task ToggleInfrared(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.InfraredLight, enabled);
        }

        public async Task TogglePrivacyMode(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Privacy, enabled);
        }

        public async Task ToggleSleepMode(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Sleep, enabled);
        }

        public async Task ToggleMobileTracking(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.MobileTracking, enabled);
        }

        public async Task<AlarmDetectionMethod> GetAlarmDetectionMethod()
        {
            AlarmDetectionMethod = await client.GetAlarmDetectionMethod(SerialNumber);
            return AlarmDetectionMethod;
        }

        public async Task GetExtraInformation()
        {
            await GetAlarmDetectionMethod();
            await GetDetectionSensibilityAsync();
        }
    }
}
