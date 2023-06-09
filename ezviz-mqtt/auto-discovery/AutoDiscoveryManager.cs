﻿using ezviz.net.domain.deviceInfo;
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
using System.Xml.Linq;
using ezviz_mqtt.commands;

namespace ezviz_mqtt.auto_discovery;

internal class AutoDiscoveryManager
{
    private const string homeAssistantDiscoveryTopic = "homeassistant/{device_class}/{node_id}/{unique_id}/config";

    private readonly ILogger logger;
    private readonly IMqttHandler mqttHandler;
    private readonly TopicExtensions topics;
    private readonly MqttOptions mqttConfig;

    JsonSerializerOptions jsonSerializationOptions = new JsonSerializerOptions()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public AutoDiscoveryManager(ILogger logger, IMqttHandler mqttHandler, TopicExtensions topics, MqttOptions mqttConfig)
    {
        this.logger = logger;
        this.mqttHandler = mqttHandler;
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

        SendDiscoveryMessage(MapToBinarySensor(StateEntities.UpgradeAvailable, "Upgrade Available", camera, device));
        SendDiscoveryMessage(MapToBinarySensor(StateEntities.UpgradeInProgress, "Upgrade In Progress", camera, device));
        SendDiscoveryMessage(MapToNumberSensor(StateEntities.UpgradePercent, "Upgrade Percent",null,"%", camera, device));
        SendDiscoveryMessage(MapToBinarySensor(StateEntities.RtspEncrypted, "RTSP Encrypted", camera, device));
        SendDiscoveryMessage(MapToNumberSensor(StateEntities.BatteryLevel, "Battery Level", "battery", "%", camera, device));
        SendDiscoveryMessage(MapToSensor(StateEntities.PirStatus, "PiR Status", camera, device));
        SendDiscoveryMessage(MapToNumberSensor(StateEntities.DiskCapacity, "Disk Capacity", "data_size", "GB", camera, device));
        SendDiscoveryMessage(MapToSensor(StateEntities.LastAlarm, "Last Alarm", camera, device));
        SendDiscoveryMessage(MapToSensor(StateEntities.AlarmScheduleEnabled, "Alarm Schedule Enabled", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.Sleeping, "Sleeping", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.AudioEnabled, "Audio", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.InfraredEnabled, "Infrared", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.StatusLedEnabled, "Status LED", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.MobileTrackingEnabled, "Mobile Tracking", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.NotifyWhenOffline, "Notify When Offline", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.Armed, "Armed", camera, device));
        SendDiscoveryMessage(MapToSwitch(StateEntities.TriggerAlarm, "Trigger Alarm", camera, device));
    }

    private void SendDiscoveryMessage(Entity entity)
    {
        mqttHandler.SendMqtt(MapDiscoveryTopic(entity), entity);
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

    private Select MapToSelect<T>(EzvizCamera camera, Device device) where T : struct
    {
        var configItem = typeof(T).Name;
        var options = Enum.GetNames(typeof(T));
        var stateTopic = topics.GetStatusTopic<T>(camera);
        var commandTopic = topics.GetStatusSetTopic<T>(camera);
        return new Select($"ezviz_{camera.SerialNumber}_{configItem.ToLower()}", $"{camera.DeviceInfo.Name} {configItem}", device, commandTopic, stateTopic, options)
        {
            Availability = GetAvailability(camera.SerialNumber),
        };
    }

    private Switch MapToSwitch(string name, string friendlyName, EzvizCamera camera, Device device)
    {
        var stateTopic = topics.GetStatusTopic(name, camera);
        var commandTopic = topics.GetStatusSetTopic(name, camera);
        return new Switch($"ezviz_{camera.SerialNumber}_{name}", $"{camera.DeviceInfo.Name} {friendlyName}", device, commandTopic, stateTopic)
        {
            Availability = GetAvailability(camera.SerialNumber),
        };
    }

    private Sensor MapToSensor(string name, string friendlyName, EzvizCamera camera, Device device)
    {
        var topic = topics.GetStatusTopic(name, camera);
        return new Sensor($"ezviz_{camera.SerialNumber}_{name}", $"{camera.DeviceInfo.Name} {friendlyName}", device, topic)
        {
            Availability = GetAvailability(camera.SerialNumber),
        };
    }

    private Sensor MapToNumberSensor(string name, string friendlyName, string? deviceClass, string? unitOfMeasure, EzvizCamera camera, Device device)
    {
        var sensor = MapToSensor(name, friendlyName, camera, device);
        sensor.DeviceClass = deviceClass;
        sensor.UnitOfMeasurement = unitOfMeasure;
        return sensor;

    }

    private Binary_Sensor MapToBinarySensor(string name, string friendlyName, EzvizCamera camera, Device device)
    {
        var topic = topics.GetStatusTopic(name, camera);
        return new Binary_Sensor($"ezviz_{camera.SerialNumber}_{name}", $"{camera.DeviceInfo.Name} {friendlyName}", device, topic)
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
