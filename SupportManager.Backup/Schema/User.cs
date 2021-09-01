using System.Collections.Generic;

namespace SupportManager.Backup.Schema
{
    internal record User(int Id, string DisplayName, string Login, List<UserEmailAddress> EmailAddresses,
        List<UserPhoneNumber> PhoneNumbers, List<ApiKey> ApiKeys, int? PrimaryEmailAddressId, int? PrimaryPhoneNumberId,
        List<TeamMember> Memberships);
}