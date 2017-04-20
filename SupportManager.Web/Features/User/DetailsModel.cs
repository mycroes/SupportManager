using System.Collections.Generic;

namespace SupportManager.Web.Features.User
{
    public class DetailsModel
    {
        public string DisplayName { get; set; }
        public int? PrimaryEmailAddressId { get; set; }
        public string PrimaryEmailAddressValue { get; set; }
        public int? PrimaryPhoneNumberId { get; set; }
        public string PrimaryPhoneNumberLabel { get; set; }
        public string PrimaryPhoneNumberValue { get; set; }
        public List<PhoneListModel> PhoneNumbers { get; set; }
        public List<EmailAddressListModel> EmailAddresses { get; set; }
    }
}