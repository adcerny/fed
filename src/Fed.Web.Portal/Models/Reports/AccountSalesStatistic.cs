namespace Fed.Web.Portal.Models.Reports
{
    public class AccountSalesStatistic
    {
        public int AccountTypeId { get; set; }
        public string AccountType { get; set; }
        public int Week { get; set; }
        public decimal Sales { get; set; }
        public int Deliveries { get; set; }
    }
}