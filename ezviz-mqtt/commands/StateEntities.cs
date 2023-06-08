using ezviz.net.domain.deviceInfo;
using ezviz.net.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_mqtt.commands
{
    internal class StateEntities
    {
        public const string AlarmSound = "AlarmSound";
        public const string AlarmDetectionMethod = "AlarmDetectionMethod";
        public const string DetectionSensitivityLevel = "DetectionSensitivityLevel";
        public const string UpgradeAvailable = "upgrade_available";
        public const string UpgradeInProgress = "upgrade_in_progress";
        public const string UpgradePercent = "upgrade_percent";
        public const string RtspEncrypted = "rtsp_encrypted";
        public const string BatteryLevel = "battery_level";
        public const string PirStatus = "pir_status";
        public const string DiskCapacity = "disk_capacity";
        public const string LastAlarm = "last_alarm";
        public const string AlarmScheduleEnabled = "alarm_schedule_enabled";
        public const string Sleeping = "sleeping";
        public const string AudioEnabled = "audio_enabled";
        public const string InfraredEnabled = "infrared_enabled";
        public const string StatusLedEnabled = "status_led_enabled";
        public const string MobileTrackingEnabled = "mobile_tracking_enabled";
        public const string NotifyWhenOffline = "notify_when_offline";
        public const string Armed = "armed";
        public const string TriggerAlarm = "trigger_alarm";

    }
}
