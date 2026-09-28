using System;
using System.Linq;
using FluentValidation;
using SupportManager.DAL;

namespace SupportManager.Web.Api.Shared
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, int> BelongsToTeamMember<T>(this IRuleBuilder<T, int> ruleBuilder,
            SupportManagerContext db, Func<T, int> teamId) =>
            ruleBuilder.Must((instance, phoneNumberId) => db.TeamMembers.Any(m =>
                    m.TeamId == teamId(instance) && m.User.PhoneNumbers.Any(p => p.Id == phoneNumberId)))
                .WithMessage("Phone number does not belong to a member of the team.");
    }
}
