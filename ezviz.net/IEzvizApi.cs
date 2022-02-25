
using ezviz.net.domain;
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
    Task<LoginResponse> Login([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Get("/v3/configurations/system/info")]
    Task<ServiceUrlsResponse> GetServiceUrls([Header("sessionId")] string sessionId);

    [Get("/v3/userdevices/v1/resources/pagelist")]
    Task<PagedListResponse> GetPagedList([Header("sessionId")] string sessionId, string filter);

    [Post("/api/device/queryAlgorithmConfig")]
    Task<DetectionSensibilityResponse> GetDetectionSensibility([Header("sessionId")] string sessionId, [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Put("/v3/devices/{serialNumber}/alarm/sound")]
    Task<IApiResponse> SetAlarmSoundLevel([Header("sessionId")] string sessionId, string serialNumber, [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
}

