namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DeviceGuardConfig
    {
        public DeviceGuardConfig(string accessToken, string refreshToken, string subject)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.Subject = subject;
        }

        public DeviceGuardConfig(string accessToken, string refreshToken)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
        }

        public DeviceGuardConfig()
        {
        }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string Subject { get; set; }
    }
}
