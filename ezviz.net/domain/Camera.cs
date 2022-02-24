using ezviz.net.domain.deviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    public class Camera : Device
    {
        internal Camera(EzvizDeviceInfo deviceInfo, PagedListResponse response) : base(deviceInfo, response)
        {
        }

        public string LocalIp => Wifi.Address ?? Connection.LocalIp ?? "0.0.0.0";
    }
}
