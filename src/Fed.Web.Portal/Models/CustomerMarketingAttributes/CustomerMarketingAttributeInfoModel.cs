using System;
using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.CustomerMarketingAttributes
{
    public class CustomerMarketingAttributeInfoModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(250)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }

        public CustomerMarketingAttributeInfoModel(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
