using System;

namespace Fed.AzureFunctions.Entities
{
    public class XeroConfig
    {
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public string Certificate { get; private set; }
        public string CertificatePassword { get; private set; }

        public static XeroConfig LoadFromEnvironment()
        {
            return new XeroConfig
            {
                ConsumerKey = Environment.GetEnvironmentVariable("xero-consumer-key"),
                ConsumerSecret = Environment.GetEnvironmentVariable("xero-consumer-secret"),
                Certificate = Environment.GetEnvironmentVariable("xero-certificate"),
                CertificatePassword = Environment.GetEnvironmentVariable("xero-certificate-password")
            };
        }
    }
}