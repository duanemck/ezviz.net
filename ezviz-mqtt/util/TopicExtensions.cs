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

        public string GetStatusTopic<T>(Camera camera) where T : struct
        {
            return GetStatusTopic(typeof(T).Name.ToLower(), camera);
        }

        public string GetStatusTopic(string configItem, Camera camera)
        {
            return GetTopic(Topics.Status, camera.SerialNumber).Replace("{entity}", configItem.ToLower());
        }

        public string GetStatusSetTopic<T>(Camera camera) where T : struct
        {
            return GetStatusSetTopic(typeof(T).Name.ToLower(), camera);
        }

        public string GetStatusSetTopic(string configItem, Camera camera)
        {
            return GetTopic(Topics.Status, camera.SerialNumber).Replace("{entity}", $"{configItem.ToLower()}/set");
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
