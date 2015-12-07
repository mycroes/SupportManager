using System.ComponentModel.DataAnnotations;
using MediatR;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class PhoneNumberCreateCommand : IRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Label { get; set; }
        [Required]
        [RegularExpression(@"^\+\d+$")]
        public string PhoneNumber { get; set; }
    }
}