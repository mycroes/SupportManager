using System.ComponentModel.DataAnnotations;

namespace SupportManager.Web.Features.User
{
    public class PhoneNumberCreateModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Label { get; set; }
        [Required]
        [RegularExpression(@"^\+31\d{9}$")]
        public string Value { get; set; }
    }
}