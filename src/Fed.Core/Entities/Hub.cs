using System;

namespace Fed.Core.Entities
{
    public class Hub
    {
        public Hub(
            Guid id,
            string name,
            string postcode,
            string addressLine1,
            string addressLine2,
            TimeSpan orderDeadline,
            DateTime createdDate)
        {
            Id = id;
            Name = name;
            Postcode = postcode;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            OrderDeadline = orderDeadline;
            CreatedDate = createdDate;
        }

        public Guid Id { get; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public TimeSpan OrderDeadline { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}