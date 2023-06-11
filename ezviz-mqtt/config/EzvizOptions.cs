namespace ezviz_mqtt.config;
public class EzvizOptions
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public bool EnablePushNotifications { get; set; } = false;
}

