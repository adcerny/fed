using Fed.Api.External.BraintreeService;
using Fed.Core.Common;
using Fed.Core.Services;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Extensions.Logging;

namespace Fed.Infrastructure.Factories
{
    public static class PaymentProcessingFactory
    {
        public static ProcessPaymentsService CreateProcessPaymentsService(
            string braintreeEnvironment,
            string braintreeMerchantId,
            string braintreeMerchantAccountId,
            string braintreePublicKey,
            string braintreePrivateKey,
            string connectionString,
            int paymentMaxConcurrentOperations,
            ILogger logger) =>
                new ProcessPaymentsService(
                    CreateBraintreeGatewayService(
                        braintreeEnvironment,
                        braintreeMerchantId,
                        braintreeMerchantAccountId,
                        braintreePublicKey,
                        braintreePrivateKey,
                        connectionString,
                        logger),
                    CreatePaymentRequestsHandler(connectionString),
                    CreateDeliveriesHandler(connectionString),
                    CreateContactsHandler(connectionString),
                    logger,
                    paymentMaxConcurrentOperations);

        public static BraintreeGatewayService CreateBraintreeGatewayService(
            string braintreeEnvironment,
            string braintreeMerchantId,
            string braintreeMerchantAccountId,
            string braintreePublicKey,
            string braintreePrivateKey,
            string connectionString,
            ILogger logger) =>
                new BraintreeGatewayService(
                    braintreeEnvironment,
                    braintreeMerchantId,
                    braintreeMerchantAccountId,
                    braintreePublicKey,
                    braintreePrivateKey,
                    CreatePaymentRequestsHandler(connectionString),
                    logger);

        private static PaymentRequestsHandler CreatePaymentRequestsHandler(string connectionString)
            => new PaymentRequestsHandler(new SqlServerConfig(connectionString));

        private static DeliveriesHandler CreateDeliveriesHandler(string connectionString)
            => new DeliveriesHandler(new SqlServerConfig(connectionString));

        private static ContactsHandler CreateContactsHandler(string connectionString)
            => new ContactsHandler(new SqlServerConfig(connectionString), new RandomIdGenerator());
    }
}
