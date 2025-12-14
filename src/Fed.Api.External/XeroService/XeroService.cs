using Microsoft.Extensions.Logging;
using System;
using Xero.Api.Core;

namespace Fed.Api.External.XeroService
{
    public class XeroService
    {
        protected readonly IXeroCoreApi _api;
        protected readonly ILogger _logger;
        private readonly XeroSettings _settings;


        public XeroService(XeroSettings settings, ILogger logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _api = new PrivateApp(_settings).Api;
        }
    }
}
