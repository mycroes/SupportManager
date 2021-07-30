using System.IO;
using System.Threading.Tasks;

namespace SupportManager.Control
{
    public interface IConnection
    {
        Task OpenAsync();
        Stream GetStream();
        void Close();
    }
}