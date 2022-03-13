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
    /// <summary>
    /// Represents an ezviz camera
    /// </summary>
    public class Camera : Device
    {
        private readonly EzvizClient client;

        internal Camera(EzvizDeviceInfo deviceInfo, PagedListResponse response, EzvizClient client) : base(deviceInfo, response)
        {
            this.client = client;
        }

        /// <summary>
        /// ezviz assigned unique serial number
        /// </summary>
        public string? SerialNumber => DeviceInfo?.DeviceSerial;

        /// <summary>
        /// User defined camera name
        /// </summary>
        public string? Name => DeviceInfo?.Name;

        /// <summary>
        /// ezviz model name
        /// </summary>
        public string? DeviceType => $"{DeviceInfo?.DeviceCategory} {DeviceInfo?.DeviceSubCategory}";

        /// <summary>
        /// IP on the user's local network
        /// </summary>
        public string? LocalIp => Wifi?.Address ?? Connection?.LocalIp ?? "0.0.0.0";
        
        /// <summary>
        /// Alarm intensity level
        /// </summary>
        public AlarmSound? AlarmSoundLevel => Status?.AlarmSoundMode;
                
        /// <summary>
        /// Is an armed/disarmed schedule enabled for this camera
        /// </summary>
        public bool AlarmScheduleEnabled => TimePlans?.Any(tp => tp.Type == 2 && tp.Enable == 1) ?? false;

        /// <summary>
        /// Is a firmware upgrade available
        /// </summary>
        public bool UpgradeAvailable => Upgrade?.IsNeedUpgrade == 1;

        /// <summary>
        /// Is a firmware upgrade in progress
        /// </summary>
        public bool UpgradeInProgress => Status?.UpgradeStatus == 0;

        /// <summary>
        /// How far along is the firmware upgrade
        /// </summary>
        public decimal UpgradePercent => Status?.UpgradeProcess ?? 0M;

        /// <summary>
        /// Is the camera in sleep mode
        /// </summary>
        public bool Sleeping => Switches?
            .Where(sw => sw.Type == SwitchType.Sleep || sw.Type == SwitchType.AutoSleep)
            .Any(sw => sw.Enable) ?? false;
        
        /// <summary>
        /// Is privacy mode enabled?
        /// </summary>
        public bool PrivacyModeEnabled => Switches?.Any(sw => sw.Type == SwitchType.Privacy && sw.Enable) ?? false;

        /// <summary>
        /// Is audio enabled?
        /// </summary>
        public bool AudioEnabled => Switches?.Any(sw => sw.Type == SwitchType.Sound && sw.Enable) ?? false;

        /// <summary>
        /// Is the infrared LED enabled
        /// </summary>
        public bool InfraredEnabled => Switches?.Any(sw => sw.Type == SwitchType.InfraredLight && sw.Enable) ?? false;

        /// <summary>
        /// Is the status LED enabled
        /// </summary>
        public bool StateLedEnabled => Switches?.Any(sw => sw.Type == SwitchType.Light && sw.Enable) ?? false;

        /// <summary>
        /// Is mobile tracking enabled
        /// </summary>
        public bool MobileTrackingEnabled => Switches?.Any(sw => sw.Type == SwitchType.MobileTracking && sw.Enable) ?? false;

        /// <summary>
        /// Is the camera armed - i.e. motion detection results in notification and siren (depending on configuration)
        /// </summary>
        public bool Armed => Status?.GlobalStatus == 1;

        /// <summary>
        /// Should the API notify users when this camera goes offline
        /// </summary>
        public bool NotifyOffline => DeviceInfo?.OfflineNotify == 1;
        
        /// <summary>
        /// Is the RTSP stream encrypted with a password
        /// </summary>
        public bool IsEncrypted => Status?.IsEncrypt == 1;

        /// <summary>
        /// Internet IP, not sure how to use it yet
        /// </summary>
        public string? WANIp => Connection?.NetIp;

        /// <summary>
        /// Unique network address
        /// </summary>
        public string? MacAddress => DeviceInfo?.Mac;

        /// <summary>
        /// Port used for local RTSP
        /// </summary>
        public int? LocalRtspPort => Connection?.LocalRtspPort;

        /// <summary>
        /// The channel this camera is on
        /// </summary>
        public int? SupportedChannels => DeviceInfo?.ChannelNumber;

        /// <summary>
        /// For battery operated cameras, the battery level
        /// </summary>
        public int? BatteryLevel => (Status != null && Status.Optionals.ContainsKey("powerRemaining")) ? int.Parse(Status.Optionals["powerRemaining"]) : 0;

        /// <summary>
        /// PiR status
        /// </summary>
        public int PirStatus => Status?.PirStatus ?? 0;

        /// <summary>
        /// Sensitivty of motion detection
        /// </summary>
        public DetectionSensitivityLevel DetectionSensitivity { get; set; }

        /// <summary>
        /// Camera is online with the ezviz API
        /// </summary>
        public bool? Online => Status != null && Status.Optionals.ContainsKey("OnlineStatus") ? Status.Optionals["OnlineStatus"] == "1" : null;
        
        
        /// <summary>
        /// SD Card space remaining in MB
        /// </summary>
        public decimal DiskCapacityMB => Status != null && Status.Optionals.ContainsKey("diskCapacity")
            ? decimal.Parse(Status.Optionals["diskCapacity"].Split(",").First())
            : 0M;
        
        /// <summary>
        /// SD Card space remaining in GB
        /// </summary>
        public decimal DiskCapacityGB => DiskCapacityMB / 1024;

        /// <summary>
        /// Motion detection method (human/vehicle/both/regular motion)
        /// </summary>
        public AlarmDetectionMethod AlarmDetectionMethod { get; set; }
        
        /// <summary>
        /// Image filters being applied
        /// </summary>
        public DisplayMode ImageDisplayMode { get; set; }

        /// <summary>
        /// Most recent alarm raised on this camera
        /// </summary>
        /// <returns></returns>
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


        internal async Task<DetectionSensitivityLevel> GetDetectionSensitivity()
        {
            DetectionSensitivity = DetectionSensitivityLevel.Unknown;
            if (Switches?.FirstOrDefault(s => s.Type == SwitchType.AutoSleep)?.Enable ?? false)
            {
                DetectionSensitivity = DetectionSensitivityLevel.Hibernate;
            }
            else
            {
                var algorithms = await client.GetDetectionSensitivity(SerialNumber);
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

        /// <summary>
        /// Update the motion detection sensitivity level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task SetDetectionSensibility(DetectionSensitivityLevel level)
        {
            var type = (DeviceInfo?.DeviceCategory == DeviceCategories.BATTERY_CAMERA_DEVICE_CATEGORY)
                        ? 3
                        : 0;
            await client.SetDetectionSensitivity(SerialNumber, type, level);
            DetectionSensitivity = level;
        }

        /// <summary>
        /// Arm the motion detection alert and siren on the camera
        /// </summary>
        /// <returns></returns>
        public async Task Arm()
        {
            await client.SetCameraArmed(SerialNumber, true);
        }

        /// <summary>
        /// Disarm the motion detection alert and siren on the camera
        /// </summary>
        /// <returns></returns>
        public async Task Disarm()
        {
            await client.SetCameraArmed(SerialNumber, false);
        }

        /// <summary>
        /// Set siren sound intensity
        /// </summary>
        /// <param name="soundLevel"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task SetAlarmSoundLevel(AlarmSound soundLevel, bool enabled)
        {
            await client.SetAlarmSoundLevel(SerialNumber, enabled, soundLevel);

            Status.AlarmSoundMode = soundLevel;
        }

        /// <summary>
        /// Get recent alarms for this camera
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Alarm>> GetAlarms()
        {
            return await client.GetAlarms(SerialNumber, false);
        }

        internal async Task ToggleSwitch(SwitchType type, bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, type, enabled);
        }

        /// <summary>
        /// Toggle audio enabled/disabled
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task ToggleAudio(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Sound, enabled);
        }

        /// <summary>
        /// Toggle status LED
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task ToggleLed(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Light, enabled);
        }

        /// <summary>
        /// Toggle infrared LED
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task ToggleInfrared(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.InfraredLight, enabled);
        }

        /// <summary>
        /// Toggle the privacy mode
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task TogglePrivacyMode(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Privacy, enabled);
        }

        /// <summary>
        /// Toggle if camera can sleep
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task ToggleSleepMode(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.Sleep, enabled);
        }

        /// <summary>
        /// Toggle mobile tracking of camera
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task ToggleMobileTracking(bool enabled)
        {
            await client.ChangeSwitch(SerialNumber, SwitchType.MobileTracking, enabled);
        }

        internal async Task<AlarmDetectionMethod> GetAlarmDetectionMethod()
        {
            AlarmDetectionMethod = await client.GetAlarmDetectionMethod(SerialNumber);
            return AlarmDetectionMethod;
        }

        internal async Task<DisplayMode> GetImageDisplayMode()
        {
            ImageDisplayMode = await client.GetImageDisplayMode(SerialNumber);
            return ImageDisplayMode;
        }

        /// <summary>
        /// Set the method used to detect motion
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task SetAlarmDetectionMethod(AlarmDetectionMethod method)
        {
            await client.SetAlarmDetectionMethod(SerialNumber, method);
            AlarmDetectionMethod = method;
        }

        /// <summary>
        /// Set image filters
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task SetImageDisplayMode(DisplayMode mode)
        {
            await client.SetImageDisplayMode(SerialNumber, mode);
            ImageDisplayMode = mode;
        }

        internal async Task GetExtraInformation()
        {
            await Task.WhenAll(new Task[] {
                 GetAlarmDetectionMethod(),
                 GetDetectionSensitivity(),
                 GetImageDisplayMode()
            });
        }

        public async Task TriggerAlarm()
        {
            await client.SendAlarm(SerialNumber, true);
        }

        public async Task StopAlarm()
        {
            await client.SendAlarm(SerialNumber, false);
        }
    }
}
