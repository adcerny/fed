using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.CustomerMarketingAttributes
{
    public class CreateCustomerMarketingAttributeModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public static CreateCustomerMarketingAttributeModel CreateEmpty() =>
           new CreateCustomerMarketingAttributeModel
           {
               Name = string.Empty,
               Description = string.Empty
           };
    }
}
