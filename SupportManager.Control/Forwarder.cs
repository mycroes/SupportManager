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
        private readonly SupportManagerContext db;
        private readonly TaskFactory exclusiveTaskFactory;

        public Forwarder(SupportManagerContext db, TaskFactory exclusiveTaskFactory)
        {
            this.db = db;
            this.exclusiveTaskFactory = exclusiveTaskFactory;
        }

        public async Task ApplyScheduledForward(int scheduledForwardId)
        {
            var scheduledForward = db.ScheduledForwards.Include(fwd => fwd.Team)
                .Include(fwd => fwd.PhoneNumber)
                .Single(fwd => fwd.Id == scheduledForwardId);

            if (scheduledForward.Deleted || scheduledForward.ScheduleId == null) return;

            await exclusiveTaskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(scheduledForward.Team.ComPort))
                    helper.ForwardTo(scheduledForward.PhoneNumber.Value);
            });
        }

        public async Task ApplyForward(int teamId, int userPhoneNumberId)
        {
            var team = db.Teams.Find(teamId);
            var userPhoneNumber = db.UserPhoneNumbers.Find(userPhoneNumberId);

            if (team == null || userPhoneNumber == null) return;

            await exclusiveTaskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(team.ComPort))
                    helper.ForwardTo(userPhoneNumber.Value);
            });
        }

        public async Task ReadAllTeamStatus()
        {
            foreach (var team in db.Teams.ToList())
            {
                var number = await exclusiveTaskFactory.StartNew(() =>
                {
                    using (var helper = new ATHelper(team.ComPort)) return helper.GetForwardedPhoneNumber();
                });

                var state = await db.ForwardingStates.OrderByDescending(s => s.When).FirstOrDefaultAsync();
                if (state?.RawPhoneNumber == number) continue;

                var newState = new ForwardingState
                {
                    Team = team,
                    When = DateTimeOffset.Now,
                    RawPhoneNumber = number,
                    DetectedPhoneNumber = db.UserPhoneNumbers
                        .FirstOrDefault(p => p.Value == number)
                };

                db.ForwardingStates.Add(newState);
                db.SaveChanges();
            }
        }
    }
}