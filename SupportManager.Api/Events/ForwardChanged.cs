using SupportManager.Api.Teams;

namespace SupportManager.Api.Events
{
    public class ForwardChanged
    {
        public Team Team { get; set; }
        public ForwardRegistration OldForward { get; set; }
        public ForwardRegistration NewForward { get; set; }
    }
}
