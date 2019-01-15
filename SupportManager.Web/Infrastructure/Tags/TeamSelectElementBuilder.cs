using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure.Tags
{
    public class TeamSelectElementBuilder : EntitySelectElementBuilder<SupportTeam>
    {
        protected override int GetValue(SupportTeam instance)
        {
            return instance.Id;
        }

        protected override string GetDisplayValue(SupportTeam instance)
        {
            return instance.Name;
        }
    }
}