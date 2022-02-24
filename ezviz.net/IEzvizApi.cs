﻿
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
}

