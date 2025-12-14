using Xero.Api;

namespace Fed.Api.External.XeroService
{
    public class XeroSettings : IXeroApiSettings
    {
        public XeroSettings(string consumerKey,
                            string consumerSecret,
                            string certificateBase64String,
                            string signingCertificatePassword)
        {
            BaseUrl = "https://api.xero.com";
            CallbackUrl = string.Empty;
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            CertificateBase64String = certificateBase64String;
            SigningCertificatePassword = signingCertificatePassword;
            AppType = XeroApiAppType.Private;
        }

        public string BaseUrl { get; }
        public string CallbackUrl { get; }
        public string ConsumerKey { get; }
        public string ConsumerSecret { get; }
        public string SigningCertificatePath { get; }
        public string SigningCertificatePassword { get; }
        public string CertificateBase64String { get; }
        public XeroApiAppType AppType { get; }
    }
}
