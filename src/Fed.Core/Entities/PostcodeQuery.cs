using System;

namespace Fed.Core.Entities
{
    public class PostcodeQuery
    {
        public PostcodeQuery(Guid? id = null)
        {
            Id = id ?? Guid.Empty;
        }

        public Guid Id { get; }
        public string Postcode { get; set; }
        public DateTime QueryDate { get; set; }
        public bool Deliverable { get; set; }
    }
}