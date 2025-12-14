namespace Fed.Web.Portal.Models.Reports
{
    public class AccountTypeStat
    {
        public string AccountType { get; set; }
        public WeeklyStat Week1 { get; set; }
        public WeeklyStat Week2 { get; set; }
        public WeeklyStat Week3 { get; set; }
        public WeeklyStat Week4 { get; set; }
    }
}