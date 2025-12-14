using System;

namespace Fed.AzureFunctions.Entities
{
    public class BraintreeConfig
    {
        public string EnvironmentName { get; private set; }
        public string MerchantId { get; private set; }
        public string MerchantAccountId { get; private set; }
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }

        public static BraintreeConfig LoadFromEnvironment()
        {
            return new BraintreeConfig
            {
                EnvironmentName = Environment.GetEnvironmentVariable("braintree-environment"),
                MerchantId = Environment.GetEnvironmentVariable("braintree-merchant-id"),
                MerchantAccountId = Environment.GetEnvironmentVariable("braintree-merchant-account-id"),
                PublicKey = Environment.GetEnvironmentVariable("braintree-public-key"),
                PrivateKey = Environment.GetEnvironmentVariable("braintree-private-key")
            };
        }
    }
}