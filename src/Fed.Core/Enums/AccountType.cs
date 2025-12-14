namespace Fed.Core.Enums
{
    public enum AccountType
    {
        /// <summary>
        /// Standard customer account
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Orders will be processed for delivery but
        /// excluded from both payment processing and
        /// invoicing.
        /// </summary>
        Internal = 1,

        /// <summary>
        /// Orders will be processed for delivery but
        /// excluded from both payment processing and
        /// invoicing.
        /// </summary>
        Presale = 2,

        /// <summary>
        /// Intended to allow us to set up an account and
        /// proposed order for a prospect, and optionally
        /// give them the log in so they can view the
        /// proposed orders. However, orders will not be
        /// processed until the prospect customer agrees
        /// to proceed and the flag is removed.
        /// </summary>
        Demo = 3,

        /// <summary>
        /// Prevents upcoming orders from being added to
        /// an account. Customer can log in but the order
        /// dashboard page shows a custom message that
        /// requires a customer to explicitly 'reactivate'
        /// the account to allow 'PlaceOrder' to work.
        /// 
        /// <para>
        /// If selected, activates a feature on the Portal
        /// customer management page to capture
        /// cancellation reason.
        /// </para>
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Prevents login, order placement and any other action.
        /// </summary>
        Deleted = 5,

        /// <summary>
        /// Customer has suspended their account until further notice.
        /// All orders are visible on the site, but will not be converted at deadline,
        /// and will not be included in supplier forecasts.
        /// </summary>
        Paused = 6
    }
}
