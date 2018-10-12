using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure
{
    public interface IBindLoggedInUser
    {
        User LoggedInUser { get; set; }
    }
}