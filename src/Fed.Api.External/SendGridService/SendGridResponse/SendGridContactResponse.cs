using System.Collections.Generic;

namespace Fed.Api.External.SendGridService.SendGridResponse
{
    public class SendGridContactResponse
    {
        public List<Result> result { get; set; }
        public int contact_count { get; set; }
        public Metadata2 _metadata { get; set; }
    }
}
