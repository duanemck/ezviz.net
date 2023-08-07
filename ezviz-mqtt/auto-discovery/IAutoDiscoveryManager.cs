using ezviz.net.domain;
using EzvizCamera = ezviz.net.domain.Camera;

namespace ezviz_mqtt.auto_discovery;

internal interface IAutoDiscoveryManager
{
    void AutoDiscoverCamera(EzvizCamera camera);
    void AutoDiscoverServiceEntities(EzvizUser user);
}
