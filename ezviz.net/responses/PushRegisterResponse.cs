using ezviz.net.domain;

namespace ezviz.net.responses
{
    internal class PushRegisterResponse
    {
        public int Status { get; set; }
        public string? Message { get; set; }

        public RegisterData? Data { get; set; }
    }

    internal class RegisterData
    {
        public string? ClientId { get; set; } 
        public string? Mqqts { get; set; }
    }
}
