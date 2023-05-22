using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_mqtt.home_assistant;

internal class Device
{
    public Device(string uniqueId, string name, string manufacturer, string model)
    {
        Identifiers.Add(uniqueId);
        Name = name;
        Manufacturer = manufacturer;
        Model = model;
    }

    public ICollection<string> Identifiers { get; set; } = new List<string>();
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string Name { get; set; }
}

