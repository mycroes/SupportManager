using System.ComponentModel.DataAnnotations;

namespace SupportManager.Web.Features.User
{
    public class EmailAddressCreateModel
    {
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Value { get; set; }
    }
}