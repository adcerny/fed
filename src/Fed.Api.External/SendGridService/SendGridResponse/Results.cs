namespace Fed.Api.External.SendGridService.SendGridResponse
{
    public class Results
    {
        public int requested_count { get; set; }
        public int created_count { get; set; }
        public int updated_count { get; set; }
        public int deleted_count { get; set; }
        public int errored_count { get; set; }
        public string errors_url { get; set; }
    }

}
