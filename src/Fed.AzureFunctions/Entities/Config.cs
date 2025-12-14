using Fed.Api.External.SendGridService;
using System;
using System.Collections.Generic;

namespace Fed.AzureFunctions.Entities
{
    public class Config
    {
        // Fed
        public string FedWebServiceUrl { get; private set; }
        public string FedPortalUrl { get; private set; }
        public IList<string> FedOpsEmailAddresses { get; private set; }
        public IList<string> FedBuyersEmailAddresses { get; private set; }
        public IList<string> FedBufferStockEmailAddresses { get; private set; }
        public string FedBufferCustomerId { get; private set; }

        // Fed Functions
        public string ProductSyncUrl { get; private set; }
        public string RaiseInvoicesUrl { get; private set; }

        // Abel & Cole
        public string AbelAndColeOrderApiUrl { get; private set; }
        public IList<string> AbelAndColeStaffingCCEmailAddresses { get; private set; }
        public IList<string> AbelAndColeStaffingToEmailAddresses { get; private set; }

        // Seven Seeded
        public IList<string> SevenSeededEmailAddresses { get; private set; }
        public string PastryCategoryId { get; private set; }

        public IList<string> YummyTummyEmailAddresses { get; private set; }

        // Merchello
        public string MerchelloApiUrl { get; private set; }
        public string MerchelloSecretToken { get; private set; }

        // Microsoft Teams
        public string TeamsWebhookUrl { get; private set; }

        // SendGrid
        public string SendGridApiKey { get; private set; }
        public string SendGridMarketingGroupId { get; private set; }
        public SendGridMarketingSettings SendGridMarketingSettings { get; set; }

        // Zedify
        public string ZedifyCsvUrl { get; private set; }
        public IList<string> ZedifyEmailAddresses { get; private set; }

        // SQL Server
        public string ConnectionString { get; private set; }

        // Braintree
        public BraintreeConfig Braintree { get; private set; }

        // Xero
        public XeroConfig Xero { get; private set; }

        // Payments
        public int PaymentMaxConcurrentOps { get; private set; }
        public string InvoiceDayOfWeek { get; private set; }

        private static List<string> LoadEmailAddressesFromEnvVar(string envVar)
        {
            var emailAddresses = new List<string>();
            var envVarValue = Environment.GetEnvironmentVariable(envVar) ?? "";
            var emails = envVarValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var email in emails)
                emailAddresses.Add(email.Trim());

            return emailAddresses;
        }

        public static Config LoadFromEnvironment()
        {
            return new Config
            {
                FedWebServiceUrl = Environment.GetEnvironmentVariable("FED_WEB_SERVICE_URL"),
                FedPortalUrl = Environment.GetEnvironmentVariable("FED_PORTAL_URL"),
                FedOpsEmailAddresses = LoadEmailAddressesFromEnvVar("FED_OPS_EMAIL_ADDRESSES"),
                FedBuyersEmailAddresses = LoadEmailAddressesFromEnvVar("FED_BUYERS_EMAIL_ADDRESSES"),
                FedBufferStockEmailAddresses = LoadEmailAddressesFromEnvVar("FED_BUFFER_STOCK_EMAIL_ADDRESSES"),
                FedBufferCustomerId = Environment.GetEnvironmentVariable("FED_BUFFER_STOCK_CUSTOMER_ID"),
                AbelAndColeOrderApiUrl = Environment.GetEnvironmentVariable("AC_ORDER_API_URL"),
                AbelAndColeStaffingCCEmailAddresses = LoadEmailAddressesFromEnvVar("AC_STAFFING_CC_EMAIL_ADDRESSES"),
                AbelAndColeStaffingToEmailAddresses = LoadEmailAddressesFromEnvVar("AC_STAFFING_TO_EMAIL_ADDRESSES"),
                ProductSyncUrl = Environment.GetEnvironmentVariable("PRODUCT_SYNC_URL"),
                RaiseInvoicesUrl = Environment.GetEnvironmentVariable("RAISE_INVOICES_URL"),
                MerchelloApiUrl = Environment.GetEnvironmentVariable("MERCHELLO_API_URL"),
                MerchelloSecretToken = Environment.GetEnvironmentVariable("MERCHELLO_SECRET_TOKEN"),
                TeamsWebhookUrl = Environment.GetEnvironmentVariable("TEAMS_WEBHOOK_URL"),
                ZedifyCsvUrl = Environment.GetEnvironmentVariable("ZEDIFY_CSV_ACCESS_URL"),
                ZedifyEmailAddresses = LoadEmailAddressesFromEnvVar("ZEDIFY_EMAIL_ADDRESSES"),
                SendGridApiKey = Environment.GetEnvironmentVariable("SEND_GRID_API_KEY"),
                SendGridMarketingGroupId = Environment.GetEnvironmentVariable("SEND_GRID_MARKETING_GROUPID"),
                SendGridMarketingSettings = new SendGridMarketingSettings
                {
                    CompanyIdFieldId = Environment.GetEnvironmentVariable("SEND_GRID_COMPANYID_FIELDID"),
                    ContactIdFieldId = Environment.GetEnvironmentVariable("SEND_GRID_CONTACTID_FIELDID"),
                    CompanyNameFieldId = Environment.GetEnvironmentVariable("SEND_GRID_COMPANYNAME_FIELDID"),
                    PrimaryContactListId = Environment.GetEnvironmentVariable("SEND_GRID_PRIMARYCONTACTS_LISTID")
                },
                ConnectionString = Environment.GetEnvironmentVariable("connection-string"),
                Braintree = BraintreeConfig.LoadFromEnvironment(),
                Xero = XeroConfig.LoadFromEnvironment(),
                PaymentMaxConcurrentOps = int.Parse(Environment.GetEnvironmentVariable("MaxConcurrentPaymentOperations") ?? "1"),
                SevenSeededEmailAddresses = LoadEmailAddressesFromEnvVar("SEVEN_SEEDED_EMAIL_ADDRESSES"),
                YummyTummyEmailAddresses = LoadEmailAddressesFromEnvVar("YUMMY_TUMMY_EMAIL_ADDRESSES"),
                PastryCategoryId = Environment.GetEnvironmentVariable("PASTRY_CATEGORY_ID")
            };
        }
    }
}