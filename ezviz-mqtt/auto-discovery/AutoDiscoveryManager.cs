using ezviz.net.domain.deviceInfo;
using ezviz_mqtt.util;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

using EzvizCamera = ezviz.net.domain.Camera;
using ezviz_mqtt.auto_discovery.domain;
using uPLibrary.Networking.M2Mqtt.Messages;
using ezviz_mqtt.config;
using Device = ezviz_mqtt.auto_discovery.domain.Device;
using ezviz.net.domain;
using Camera = ezviz_mqtt.auto_discovery.domain.Camera;
using Sensor = ezviz_mqtt.auto_discovery.domain.Sensor;
using Switch = ezviz_mqtt.auto_discovery.domain.Switch;

namespace ezviz_mqtt.auto_discovery;

internal class AutoDiscoveryManager
{
    private const string homeAssistantDiscoveryTopic = "homeassistant/{device_class}/{node_id}/{unique_id}/config";

    private readonly ILogger logger;
    private readonly MqttClient mqttClient;
    private readonly TopicExtensions topics;
    private readonly MqttOptions mqttConfig;

    JsonSerializerOptions jsonSerializationOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public AutoDiscoveryManager(ILogger logger, MqttClient mqttClient, TopicExtensions topics, MqttOptions mqttConfig)
    {
        this.logger = logger;
        this.mqttClient = mqttClient;
        this.topics = topics;
        this.mqttConfig = mqttConfig;
    }

    public void AutoDiscoverCamera(EzvizCamera camera)
    {
        var device = MapCameraToDevice(camera);

        SendDiscoveryMessage(MapCameraToHA(camera, device));

        SendDiscoveryMessage(MapToSelect<AlarmSound>(camera, device));
        SendDiscoveryMessage(MapToSelect<AlarmDetectionMethod>(camera, device));
        SendDiscoveryMessage(MapToSelect<DetectionSensitivityLevel>(camera, device));

        SendDiscoveryMessage(MapToSensor("upgrade_available", "Upgrade Available", camera, device));
        SendDiscoveryMessage(MapToSensor("upgrade_in_progress", "Upgrade In Progress", camera, device));
        SendDiscoveryMessage(MapToSensor("upgrade_percent", "Upgrade Percent", camera, device));
        SendDiscoveryMessage(MapToSensor("rtsp_encrypted", "RTSP Encrypted", camera, device));
        SendDiscoveryMessage(MapToSensor("battery_level", "Battery Level", camera, device));
        SendDiscoveryMessage(MapToSensor("pir_status", "PiR Status", camera, device));
        SendDiscoveryMessage(MapToSensor("disk_capacity", "Disk Capacity", camera, device));
        SendDiscoveryMessage(MapToSensor("last_alarm", "Last Alarm", camera, device));


        SendDiscoveryMessage(MapToSwitch("alarm_schedule_enabled", "Alarm Schedule Enabled", camera, device));
        SendDiscoveryMessage(MapToSwitch("sleeping", "Sleeping", camera, device));
        SendDiscoveryMessage(MapToSwitch("audio_enabled", "Audio", camera, device));
        SendDiscoveryMessage(MapToSwitch("infrared_enabled", "Infrared", camera, device));
        SendDiscoveryMessage(MapToSwitch("status_led_enabled", "Status LED", camera, device));
        SendDiscoveryMessage(MapToSwitch("motion_tracking_enabled", "Motion Tracking", camera, device));
        SendDiscoveryMessage(MapToSwitch("notify_when_offline", "Notify When Offline", camera, device));
        SendDiscoveryMessage(MapToSwitch("armed", "Armed", camera, device));
        SendDiscoveryMessage(MapToSwitch("trigger_alarm", "Trigger Alarm", camera, device));
    }

    private void SendDiscoveryMessage(Entity entity)
    {
        SendMqtt(MapDiscoveryTopic(entity), entity);
    }

    private void SendMqtt(string topic, object data)
    {
#pragma warning disable IL2026
        var dataString = JsonSerializer.Serialize(data, jsonSerializationOptions);
#pragma warning restore IL2026
        if (dataString != null)
        {
            logger.LogInformation($"Publishing [{dataString}] to [{topic}]");
            mqttClient.Publish(topic, Encoding.UTF8.GetBytes(dataString), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
        }
    }



    private Device MapCameraToDevice(EzvizCamera camera)
    {
        return new Device($"ezviz_{camera.SerialNumber}", camera.DeviceInfo.Name, "Ezviz", camera.DeviceType)
        {
            Connections = new List<string[]> { new string[] { "MAC", camera.MacAddress ?? "" } },
            SoftwareVersion = camera.Version
        };
    }

    private List<Availability> GetAvailability(string uniqueId)
    {
        return new List<Availability>()
        {
            new Availability(topics.GetLwtTopicForCamera(uniqueId), mqttConfig.ServiceLwtOnlineMessage, mqttConfig.ServiceLwtOfflineMessage)
        };
    }

    private Camera MapCameraToHA(EzvizCamera camera, Device device)
    {
        var Camera = new Camera($"ezviz_{camera.SerialNumber}_camera", $"{camera.DeviceInfo.Name} Camera", device, topics.GetTopic(Topics.Image, camera))
        {
            Availability = GetAvailability(camera.SerialNumber),
            Icon = "mdi:cctv",
            ImageEncoding = "b64"
        };
        return Camera;
    }

    private Select MapToSelect<T>(EzvizCamera camera, Device device)
    {
        var configItem = typeof(T).Name;
        var options = Enum.GetNames(typeof(T));
        var topic = topics.GetConfigTopic(configItem, camera);
        return new Select($"ezviz_{camera.SerialNumber}_{configItem.ToLower()}", $"{camera.DeviceInfo.Name} {configItem}", device, topic, topic, options)
        {
            Availability = GetAvailability(camera.SerialNumber),
        };
    }

    private Switch MapToSwitch(string name, string friendlyName, EzvizCamera camera, Device device)
    {
        var topic = topics.GetConfigTopic(name, camera);
        return new Switch($"ezviz_{camera.SerialNumber}_{name}", $"{camera.DeviceInfo.Name} {friendlyName}", device, topic, topic)
        {
            Availability = GetAvailability(camera.SerialNumber),
        };
    }

    private Sensor MapToSensor(string name, string friendlyName, EzvizCamera camera, Device device)
    {
        var topic = topics.GetConfigTopic(name, camera);
        return new Sensor($"ezviz_{camera.SerialNumber}_{name}", $"{camera.DeviceInfo.Name} {friendlyName}", device, topic)
        {
            Availability = GetAvailability(camera.SerialNumber),
        };
    }

    private string MapDiscoveryTopic(Entity entity)
    {
        return homeAssistantDiscoveryTopic
            .Replace("{device_class}", entity.GetComponentType())
            .Replace("{node_id}", entity.Device.Identifiers.FirstOrDefault() ?? "generic_ezviz_camera")
            .Replace("{unique_id}", entity.UniqueId);
    }


}
