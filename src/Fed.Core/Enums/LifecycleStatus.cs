namespace Fed.Core.Enums
{
    public enum LifecycleStatus
    {
        /// <summary>
        /// Any customer that has not had a delivery.
        /// <para>
        /// Applies to partially complete customer
        /// records that are missing mandatory
        /// information (e.g. delivery, paryment) and
        /// completed records with upcoming orders
        /// that have not yet been confirmed.
        /// </para>
        /// </summary>
        Prospect,

        /// <summary>
        /// Customers become active when their first 
        /// order is confirmed for delivery at the
        /// deadline.
        /// <para>
        /// With 'Presale' cannot progress to Active.
        /// </para>
        /// <para>
        /// Accounts marked as test accounts can
        /// progress through the lifecycle but are
        /// excluded from order processing.
        /// </para>
        /// </summary>
        Active,

        /// <summary>
        /// Account is marked Lapsed after 30 days
        /// since last delivery.
        /// 
        /// <para>
        /// Logon activity does not affect this status.
        /// </para>
        /// 
        /// <para>
        /// If a delivery occurs the customer returns
        /// to Active classification (It is impossible
        /// to return to being a prospect).
        /// </para>
        /// </summary>
        Lapsed
    }
}
