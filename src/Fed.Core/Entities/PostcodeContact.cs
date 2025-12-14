using System;

namespace Fed.Core.Entities
{
    public class PostcodeContact
    {
        public PostcodeContact(Guid? id = null)
        {
            Id = id ?? Guid.Empty;
        }

        public Guid Id { get; }
        public string Postcode { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsDeliverable { get; set; }
    }
}