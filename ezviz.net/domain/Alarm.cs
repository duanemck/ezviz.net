using ezviz.net.cloud_mqtt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;

public class Alarm
{
    public Alarm()
    {
        
    }

    internal Alarm(PushMessage pushMessage)
    {
        string[] alarmDetails = pushMessage.Ext.Split(",");

        AlarmId = alarmDetails[15];
        AlarmMessage = pushMessage.Alert;
        AlarmName = alarmDetails[17];

        AlarmStartTimeStr = alarmDetails[1];
        DeviceSerial = alarmDetails[2];
        AlarmType = int.Parse(alarmDetails[4]);
        PicUrl = alarmDetails.Length > 16 ? alarmDetails[16] : "";
    }

    public string AlarmId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string DeviceSerial { get; set; } = null!;
    public int ChannelNo { get; set; }
    public int ChannelType { get; set; }
    public string AlarmName { get; set; } = null!;
    public string StartTime { get; set; } = null!;
    public int AlarmType { get; set; }
    public long AlarmStartTime { get; set; }
    public DateTime AlarmStartTimeParsed => DateTime.ParseExact(AlarmStartTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    public string AlarmStartTimeStr { get; set; } = null!;
    public string Checksum { get; set; } = null!;
    public int IsCheck { get; set; }
    public int IsVideo { get; set; }
    public int IsEncrypt { get; set; }
    public int IsCloud { get; set; }
    public string PicUrl { get; set; } = null!;
    public string RecUrl { get; set; } = null!;
    public string S_PicUrl { get; set; } = null!;
    public string S_RecURl { get; set; } = null!;
    public string Remark { get; set; } = null!;
    public int RecState { get; set; }
    public string RelationId { get; set; } = null!;
    public string PicUrlGroup { get; set; } = null!;
    public string SampleName { get; set; } = null!;
    public int PreTime { get; set; }
    public int DelayTime { get; set; }
    public string CustomerType { get; set; } = null!;
    public string CustomerInfo { get; set; } = null!;
    public int WithTinyVideo { get; set; }
    public string RelationAlarm { get; set; } = null!;
    public string RelationAlarms { get; set; } = null!;
    public string AlarmMessage { get; set; } = null!;
    public int Crypt { get; set; }
    public int AnalysisType { get; set; }
    public string AnalysisResult { get; set; } = null!;
    public bool HasValueAddedService { get; set; }
    public string ShowHumanName { get; set; } = null!;

    public string? DownloadedPicture { get; set; } = null!;

    public bool IsOlderThan(int minutes) => (DateTime.Now - AlarmStartTimeParsed).TotalSeconds > (minutes * 60);
    public bool IsEarlierThan(int minutes) => !IsOlderThan(minutes);

}

