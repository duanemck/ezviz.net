using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;

public class Alarm
{
    public string AlarmId { get; set; }
    public string UserId { get; set; }
    public string DeviceSerial { get; set; }
    public int ChannelNo { get; set; }
    public int ChannelType { get; set; }
    public string AlarmName { get; set; }
    public string StartTime { get; set; }
    public int AlarmType { get; set; }
    public long AlarmStartTime { get; set; }
    public DateTime AlarmStartTimeParsed => DateTime.ParseExact(AlarmStartTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    public string AlarmStartTimeStr { get; set; }
    public string Checksum { get; set; }
    public int IsCheck { get; set; }
    public int IsVideo { get; set; }
    public int IsEncrypt { get; set; }
    public int IsCloud { get; set; }
    public string PicUrl { get; set; }
    public string RecUrl { get; set; }
    public string S_PicUrl { get; set; }
    public string S_RecURl { get; set; }
    public string Remark { get; set; }
    public int RecState { get; set; }
    public string RelationId { get; set; }
    public string PicUrlGroup { get; set; }
    public string SampleName { get; set; }
    public int PreTime { get; set; }
    public int DelayTime { get; set; }
    public string CustomerType { get; set; }
    public string CustomerInfo { get; set; }
    public int WithTinyVideo { get; set; }
    public string RelationAlarm { get; set; }
    public string RelationAlarms { get; set; }
    public string AlarmMessage { get; set; }

    public int Crypt { get; set; }
    public int AnalysisType { get; set; }
    public string AnalysisResult { get; set; }
    public bool HasValueAddedService { get; set; }
    public string ShowHumanName { get; set; }

}

