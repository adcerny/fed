
using System.ComponentModel.DataAnnotations;

namespace Fed.Core.Enums
{
    public enum DiscountEvent
    {
        [Display(Name = "Customer enters promo code")]
        CodeEntered = 0,
        [Display(Name = "Customer completes sign up")]
        SignUp = 1,
        [Display(Name = "Customer places their first order")]
        FirstOrder = 2,
        [Display(Name = "Customer places their next order")]
        NextOrder = 3,
        [Display(Name = "Assigned internally")]
        Manual = 4
    }
}
