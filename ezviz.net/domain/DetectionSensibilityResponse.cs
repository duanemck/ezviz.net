using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;
internal class DetectionSensibilityResponse
{
    public AlgorithmConfiguration AlgorithmConfig { get; set; } = null!;
    public string ResultCode { get; set; } = null!;
    public string ResultDes { get; set; } = null!;
}

internal class AlgorithmConfiguration
{
    public string Result { get; set; } = null!;
    public ICollection<Algorithm> AlgorithmList { get; set; } = null!;
}

internal class Algorithm
{
    public string Type { get; set; } = null!;
    public int Channel { get; set; } 
    public string Value { get; set; } = null!;
}