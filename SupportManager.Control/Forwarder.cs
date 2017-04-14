using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Control
{
    public class Forwarder : IForwarder
    {
        private readonly SupportManagerContext context;
        private readonly TaskFactory taskFactory;

        public Forwarder(SupportManagerContext context, TaskFactory taskFactory)
        {
            this.context = context;
            this.taskFactory = taskFactory;
        }

        public async Task ApplyScheduledForward(int scheduledForwardId)
        {
            await taskFactory.StartNew(() => ApplyScheduledForwardImpl(scheduledForwardId));
        }

        private void ApplyScheduledForwardImpl(int scheduledForwardId)
        {
            var scheduledForward = context.ScheduledForwards.Include(fwd => fwd.Team)
                .Include(fwd => fwd.PhoneNumber)
                .Single(fwd => fwd.Id == scheduledForwardId);

            if (scheduledForward.Deleted) return;

            using (var helper = new ATHelper(scheduledForward.Team.ComPort))
                helper.ForwardTo(scheduledForward.PhoneNumber.Value);
        }

        public async Task ApplyForward(int teamId, int userPhoneNumberId)
        {
            await taskFactory.StartNew(() => ApplyForwardImpl(teamId, userPhoneNumberId));
        }

        private void ApplyForwardImpl(int teamId, int userPhoneNumberId)
        {
            var team = context.Teams.Find(teamId);
            var userPhoneNumber = context.UserPhoneNumbers.Find(userPhoneNumberId);

            using (var helper = new ATHelper(team.ComPort))
                helper.ForwardTo(userPhoneNumber.Value);
        }

        public async Task ReadStatus(int teamId)
        {
            var team = context.Teams.Find(teamId);
            if (team == null) return;

            var state = context.ForwardingStates.OrderByDescending(s => s.When).FirstOrDefault();
            var helper = new ATHelper(team.ComPort);
            var number = helper.GetForwardedPhoneNumber();

            if (state?.RawPhoneNumber != number)
            {
                var newState = new ForwardingState
                {
                    Team = team,
                    When = DateTimeOffset.Now,
                    RawPhoneNumber = number,
                    DetectedPhoneNumber = context.UserPhoneNumbers
                        .FirstOrDefault(p => p.Value == number)
                };

                context.ForwardingStates.Add(newState);
                context.SaveChanges();
            }
        }
    }
}