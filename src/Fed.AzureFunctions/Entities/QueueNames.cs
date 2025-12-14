namespace Fed.AzureFunctions.Entities
{
    public static class QueueNames
    {
        public const string PaymentProcessing = "payment-processing";
        public const string ZedifyNotification = "zedify-notification";
        public const string SupplierFinalOrder = "supplier-final-order";
        public const string SendGridSync = "sendgrid-sync";
        public const string SupplierMinimumOrder = "supplier-minimum-order";
        public const string SupplierPurchaseOrder = "supplier-purchase-order";
    }
}