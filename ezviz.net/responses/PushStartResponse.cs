using ezviz.net.domain;

namespace ezviz.net.responses
{
    internal class PushStartResponse
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public string? Ticket { get; set; }
    }
}
