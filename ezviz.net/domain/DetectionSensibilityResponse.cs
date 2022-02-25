using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;
internal class DetectionSensibilityResponse
{
    public AlgorithmConfiguration AlgorithmConfig { get; set; }
    public string ResultCode { get; set; }
    public string ResultDes { get; set; }
}

internal class AlgorithmConfiguration
{
    public string Result { get; set; }
    public ICollection<Algorithm> AlgorithmList { get; set; }
}

internal class Algorithm
{
    public string Type { get; set; }
    public int Channel { get; set; }
    public string Value { get; set; }
}