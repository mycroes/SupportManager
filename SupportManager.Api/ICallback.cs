using System.Threading.Tasks;
using Refit;
using SupportManager.Api.Events;

namespace SupportManager.Api
{
    public interface ICallback
    {
        [Get("/ping")]
        Task Ping();

        [Post("/notify")]
        Task ForwardChanged([Body] ForwardChanged message);
    }
}
