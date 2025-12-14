using System;
using System.Security.Cryptography.X509Certificates;
using Xero.Api.Core;
using Xero.Api.Infrastructure.Authenticators;

namespace Fed.Api.External.XeroService
{
    public class PrivateApp
    {

        private readonly IXeroCoreApi _api;

        public PrivateApp(XeroSettings settings)
        {
            var cert = new X509Certificate2(Convert.FromBase64String(settings.CertificateBase64String),
                                            settings.SigningCertificatePassword,
                                            X509KeyStorageFlags.MachineKeySet);

            var auth = new PrivateAuthenticator(cert);

            _api = new XeroCoreApi(auth, settings);
        }

        public IXeroCoreApi Api
        {
            get { return _api; }
        }
    }
}
