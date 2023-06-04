using Camera = ezviz.net.domain.Camera;

namespace ezviz_mqtt.util
{
    internal class TopicExtensions : Dictionary<string, string>
    {

        public TopicExtensions(IDictionary<string,string> topics, StringComparer comparer) : base(topics, comparer)
        {
            
        }

        public string GetTopic(Topics key)
        {
            return this[key.ToString()];
        }


        public string GetTopic(Topics key, Camera camera)
        {
            return GetTopic(key, camera.SerialNumber);
        }

        public string GetConfigTopic(string configItem, Camera camera)
        {
            return GetTopic(Topics.Config, camera.SerialNumber).Replace("{configItem}", configItem.ToLower());
        }

        public string GetTopic(Topics key, string? serialNumber)
        {
            return GetTopic(key).Replace("{serial}", serialNumber);
        }

        public string GetLwtTopicForCamera(string? serial)
        {
            return GetTopic(Topics.LWT, serial);
        }

    }
}
