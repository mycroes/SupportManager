using System.Threading.Tasks;

namespace SupportManager.Contracts
{
    public interface IForwarder
    {
        Task ApplyScheduledForward(int id);

        Task ApplyForward(int teamId, int userPhoneNumberId);

        Task ReadStatus(int teamId);
    }
}