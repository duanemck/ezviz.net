
using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using Refit;

namespace ezviz.net;

[Headers(
    "appId:ys7",
    "clientNo:web_site",
    "clientType:3",
    "customNo:1000001",
    "featureCode:1fc28fa018178a1cd1c091b13b2f9f02",
    "language:enGB",
    "netType:WIFI",
    "lang:eng"
)]
internal interface IEzvizApi
{
    [Post("/v3/users/login/v5")]
    Task<LoginResponse> Login([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data, [Property(SessionIdDelegatingHandler.PROPERTY_ANON)] bool anon = true);

    [Get("/v3/configurations/system/info")]
    Task<ServiceUrlsResponse> GetServiceUrls();

    [Get("/v3/userdevices/v1/resources/pagelist")]
    Task<PagedListResponse> GetPagedList(string filter, CancellationToken stoppingToken = default);

    [Post("/api/device/queryAlgorithmConfig")]
    Task<DetectionSensibilityResponse> GetDetectionSensitivity(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data, CancellationToken stoppingToken = default);

    [Put("/v3/devconfig/v1/sensitivity/{deviceSerial}/{channel}/{sensitivityType}/{value}")]
    Task<GenericResponse> SetDetectionSensitivity(string deviceSerial, int channel, int sensitivityType, int value);

    [Put("/v3/devices/{serialNumber}/alarm/sound")]
    Task<IApiResponse> SetAlarmSoundLevel(string serialNumber, 
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Get("/v3/alarms/v2/advanced")]
    Task<AlarmInfoResponse> GetAlarmInformation(IDictionary<string,object> query, CancellationToken stoppingToken = default);


    [Put("/v3/devices/{serialNumber}/1/1/{switchType}/switchStatus")]
    Task<GenericResponse> ChangeSwitch(string serialNumber,
        SwitchType switchType, [Body(BodySerializationMethod.UrlEncoded)] IDictionary<string, object> body);


    [Post("/v3/userdevices/v1/group/switchDefenceMode")]
    Task <SetDefenceModeResponse> SetDefenceMode(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Post("/v3/userdevices/v1/group/defenceMode?groupId=-1")]
    Task<GetDefenceModeResponse> GetDefenceMode();

    [Put("/v3/devices/{deviceSerial}/{channel}/changeDefenceStatusReq")]
    Task<SetCameraArmedModeResponse> ChangeCameraArmedStatus(string deviceSerial, int channel,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Get("/v3/devconfig/v1/keyValue/{deviceSerial}/{channel}/op")]
    Task<GetDeviceConfigResponse> GetDeviceConfig(string deviceSerial, int channel, [Query("key")] string key);

    [Put("/v3/devconfig/v1/keyValue/{deviceSerial}/{channel}/op")]
    Task<GenericResponse> SetDeviceConfig(string deviceSerial, int channel,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Post("/v3/devices/{deviceSerial}/alarm/setChannelWhistle")]
    Task<GenericResponse> SetChannelWhistle(string deviceSerial,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Put("/v3/devices/{deviceSerial}/{channel}/sendAlarm")]
    Task<GenericResponse> SendAlarm(string deviceSerial, int channel, int enable);
}

