using System;

namespace Fed.Core.Entities
{
    public class CustomerMarketingAttribute
    {
        public CustomerMarketingAttribute(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public Guid Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
