﻿using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class P2PEndpoint
{
    public string IP { get; set; } = null!;
    public int Port { get; set; }
}

