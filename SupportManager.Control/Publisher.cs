using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;
using Refit;
using SupportManager.Api;
using SupportManager.Api.Events;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.DAL;
using User = SupportManager.Api.Users.User;

namespace SupportManager.Control
{
    internal class Publisher : IPublisher
    {
        private readonly SupportManagerContext db;

        public Publisher(SupportManagerContext db)
        {
            this.db = db;
        }

        public async Task NotifyStateChange(int? prevStateId, int nextStateId, string callbackUrl, PerformContext context)
        {
            var callback = RestService.For<ICallback>(callbackUrl);
            var prevState = prevStateId == null ? null : await MapState(prevStateId.Value);
            var nextState = await MapState(nextStateId);
            var team = await db.ForwardingStates.Where(s => s.Id == nextStateId).Select(s => s.Team)
                .Select(t => new Team {Id = t.Id, Name = t.Name}).SingleAsync();

            await callback.ForwardChanged(new ForwardChanged {OldForward = prevState, NewForward = nextState, Team = team});
        }

        private async Task<ForwardRegistration> MapState(int id)
        {
            return await db.ForwardingStates.Where(s => s.Id == id).Select(s => new ForwardRegistration
            {
                User =
                    s.DetectedPhoneNumberId == null
                        ? null
                        : new User
                        {
                            DisplayName = s.DetectedPhoneNumber.User.DisplayName,
                            Id = s.DetectedPhoneNumber.UserId
                        },
                PhoneNumber = new PhoneNumber
                {
                    Id = s.DetectedPhoneNumberId == null ? 0 : s.DetectedPhoneNumber.Id,
                    Label = s.DetectedPhoneNumber.Label,
                    Value = s.DetectedPhoneNumber.Value ?? s.RawPhoneNumber
                },
                When = s.When
            }).SingleAsync();
        }
    }
}