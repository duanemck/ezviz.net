using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace ha_autodiscovery.net;

public class AutoDiscoveryManager
{
    private readonly MqttClient client;

    public AutoDiscoveryManager(MqttClient client, Dictionary<string, string> topics)
    {
        this.client = client;
    }

    public void AutoDiscoverCamera(Camera camera)
    {

       
            var haDevice = MapCameraToDevice(camera);

            //Attributes
            /* WAN Ip
             * MacAddress
             * LocalRtspPort
             * Channel
             * 
             */
            var haCamera = MapCameraToHA(camera, haDevice);
            SendMqtt(MapDiscoveryTopic("camera", haCamera), haCamera);


            var soundLevel = MapToSelect(camera, camera.AlarmSoundLevel ?? AlarmSound.Unknown, haDevice);
            SendMqtt(MapDiscoveryTopic("select", soundLevel), soundLevel);

            /*
             * Options Sound level
             * Switch Alarm Schedule
             * Sensor Upgrade Available
             * Switch Upgrade In Progress
             * Sensor Upgrade Percent
             * Switch Sleeping
             * Swsenitch Audio Enabled
             * Switch Infrared enabled
             * Switch Status LED enabled
             * Switch Mobile trackign enabled
             * Switch Notify offline
             * Sensor RTSP encrypted
             * Sensor BatteryLevel
             * Sensor PiRStatus (Motion?)
             * Sensor Disk Capacity
             * Switch Armed
             * Options DetectionMethod
             * NumberInput Sensitivity
             * Sensor LastAlarm (Attributes)
             * Sensor AlarmManuallyActivated
             */
        



        //TODO: Think about this some more and then complete it
        return;
        //var device = new Device(homeAssistantUniqueId, "EZVIZ Mqtt Bridge", "duanemck", "");
        //var availability = new Availability(mqttConfig.ServiceLwtTopic, mqttConfig.ServiceLwtOnlineMessage, mqttConfig.ServiceLwtOfflineMessage);
        //var defenceModeEntity = new Entity()
        //{
        //    Name = "EZVIZ Defence Mode",
        //    UniqueId = $"{homeAssistantUniqueId}_defence_mode",
        //    Device = device,
        //    Availability = availability,
        //    CommandTopic = _globalCommandTopic.Replace("#", MQTT_COMMAND_DEFENCEMODE),
        //    StateTopic = _globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE),
        //    DeviceClass = "switch",
        //    PayloadOn = "Away",
        //    PayloadOff = "Home"
        //};
    }

    private string GetTopic(Topics key)
    {
        return mqttTopics[key.ToString()];
    }

    private string GetTopic(Topics key, Camera camera)
    {
        return GetTopic(key, camera.SerialNumber);
    }

    private string GetConfigTopic(string configItem, Camera camera)
    {
        return GetTopic(Topics.Config, camera.SerialNumber).Replace("{configItem}", configItem);
    }

    private string GetTopic(Topics key, string? serialNumber)
    {
        return GetTopic(key).Replace("{serial}", serialNumber);
    }

    private HADevice MapCameraToDevice(Camera camera)
    {
        return new HADevice($"ezviz_{camera.SerialNumber}", camera.DeviceInfo.Name, "Ezviz", camera.DeviceType)
        {
            Connections = new List<string[]> { new string[] { "MAC", camera.MacAddress ?? "" } },
            SoftwareVersion = camera.Version
        };
    }

    private HACamera MapCameraToHA(Camera camera, HADevice device)
    {
        var haCamera = new HACamera($"ezviz_{camera.SerialNumber}_camera", $"{camera.DeviceInfo.Name}_camera", device, GetTopic(Topics.Image, camera))
        {
            Availability = new List<Availability>()
        {
            new Availability(LwtTopicForCamera(camera.SerialNumber), mqttConfig.ServiceLwtOnlineMessage, mqttConfig.ServiceLwtOfflineMessage)
        },
            Icon = "mdi:cctv",
            ImageEncoding = "b64",
            EntityCategory = "config"
        };
        return haCamera;
    }

    private Select MapToSelect<T>(Camera camera, T value, HADevice device)
    {
        var configItem = typeof(T).Name;
        var options = Enum.GetNames(typeof(T));
        return new Select($"ezviz_{camera.SerialNumber}_{configItem}", $"{camera.DeviceInfo.Name} ${configItem}", device, GetConfigTopic(configItem, camera), options);
    }

    private string MapDiscoveryTopic(string deviceClass, Entity entity)
    {
        return homeAssistantDiscoveryTopic
            .Replace("{device_class}", deviceClass)
            .Replace("{unique_id}", entity.UniqueId);
    }


}
