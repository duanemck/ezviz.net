using ezviz.net.responses;
using Refit;

namespace ezviz.net;

internal interface IEzvizPushApi
{
    [Post("/v1/getClientId")]
    Task<PushRegisterResponse> Register([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data, [Header("Authorization")] string authToken);

    [Post("/api/push/start")]
    Task<PushStartResponse> StartPush([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);

    [Post("/api/push/stop")]
    Task<HttpResponseMessage> StopPush([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
}


