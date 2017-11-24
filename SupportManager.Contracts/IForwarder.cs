using System.Threading.Tasks;
using Hangfire.Server;

namespace SupportManager.Contracts
{
    public interface IForwarder
    {
        Task ApplyScheduledForward(int id, PerformContext context);

        Task ApplyForward(int teamId, int userPhoneNumberId, PerformContext context);

        Task ReadAllTeamStatus(PerformContext context);
    }
}