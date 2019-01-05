using System.Threading.Tasks;
using Hangfire.Server;

namespace SupportManager.Control
{
    public interface IPublisher
    {
        Task NotifyStateChange(int? prevStateId, int nextStateId, string callbackUrl, PerformContext context);
    }
}