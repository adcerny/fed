namespace Fed.Api.External.SendGridService.SendGridResponse
{
    public class SendGridJobResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public string job_type { get; set; }
        public Results results { get; set; }
        public string started_at { get; set; }
        public string finished_at { get; set; }
    }

}
