namespace SupportManager.Web.Features.User
{
    public class UserExistsResponse
    {
        public string UserName { get; set; }
        public bool IsExistingUser { get; set; }
        public DAL.User User { get; set; }
    }
}