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
        private readonly TaskFactory exclusiveTaskFactory;

        public Forwarder(SupportManagerContext context, TaskFactory exclusiveTaskFactory)
        {
            this.context = context;
            this.exclusiveTaskFactory = exclusiveTaskFactory;
        }

        public async Task ApplyScheduledForward(int scheduledForwardId)
        {
            var scheduledForward = context.ScheduledForwards.Include(fwd => fwd.Team)
                .Include(fwd => fwd.PhoneNumber)
                .Single(fwd => fwd.Id == scheduledForwardId);

            if (scheduledForward.Deleted) return;

            await exclusiveTaskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(scheduledForward.Team.ComPort))
                    helper.ForwardTo(scheduledForward.PhoneNumber.Value);
            });
        }

        public async Task ApplyForward(int teamId, int userPhoneNumberId)
        {
            var team = context.Teams.Find(teamId);
            var userPhoneNumber = context.UserPhoneNumbers.Find(userPhoneNumberId);

            if (team == null || userPhoneNumber == null) return;

            await exclusiveTaskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(team.ComPort))
                    helper.ForwardTo(userPhoneNumber.Value);
            });
        }

        public async Task ReadAllTeamStatus()
        {
            foreach (var team in context.Teams.ToList())
            {
                var number = await exclusiveTaskFactory.StartNew(() =>
                {
                    using (var helper = new ATHelper(team.ComPort)) return helper.GetForwardedPhoneNumber();
                });

                var state = await context.ForwardingStates.OrderByDescending(s => s.When).FirstOrDefaultAsync();
                if (state?.RawPhoneNumber == number) continue;

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