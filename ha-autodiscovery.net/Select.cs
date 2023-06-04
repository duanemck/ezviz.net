namespace ha_autodiscovery.net
{
    public class Select : Entity
    {

        public Select(string name, string uniqueId, Device device, string commandTopic, IEnumerable<string> options) : base(name, uniqueId, device)
        {
            CommandTopic = commandTopic;
            Options = options;
        }

        public string CommandTopic { get; }
        public IEnumerable<string> Options { get; }
    }
}
