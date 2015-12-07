using System.Collections.Generic;

namespace SupportManager.Web.Features.User
{
    public class DetailsModel
    {
        public string Name { get; set; }
        public string PrimaryEmailAddressValue { get; set; }
        public string PrimaryPhoneNumberLabel { get; set; }
        public string PrimaryPhoneNumberValue { get; set; }
        public List<PhoneListModel> PhoneNumbers { get; set; }
        public List<EmailAddressListModel> EmailAddresses { get; set; }
    }
}