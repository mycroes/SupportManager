using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Control
{
    public class Forwarder : IForwarder
    {
        private readonly SupportManagerContext db;
        private readonly TaskFactory exclusiveTaskFactory;

        private static readonly ConsoleTextColor Debug = ConsoleTextColor.Green;
        private static readonly ConsoleTextColor Info = ConsoleTextColor.White;
        private static readonly ConsoleTextColor Warning = ConsoleTextColor.Yellow;
        private static readonly ConsoleTextColor Error = ConsoleTextColor.Red;

        public Forwarder(SupportManagerContext db, TaskFactory exclusiveTaskFactory)
        {
            this.db = db;
            this.exclusiveTaskFactory = exclusiveTaskFactory;
        }

        public async Task ApplyScheduledForward(int scheduledForwardId, PerformContext context)
        {
            context.WriteLine(Debug, $"Looking up scheduledForward with Id {scheduledForwardId}");
            var scheduledForward = await db.ScheduledForwards.Include(fwd => fwd.Team)
                .Include(fwd => fwd.PhoneNumber)
                .SingleAsync(fwd => fwd.Id == scheduledForwardId)
                .ConfigureAwait(false);

            context.WriteLine(Info,
                $"Found entry for team {scheduledForward.Team.Name}, forward to {scheduledForward.PhoneNumber.Value}");

            if (scheduledForward.Deleted || scheduledForward.ScheduleId == null)
            {
                context.WriteLine(Warning,
                    $"Entry was deleted (Deleted = {scheduledForward.Deleted}, ScheduleId = {scheduledForward.ScheduleId})");
                return;
            }

            await ForwardImpl(context, scheduledForward.Team.ComPort, scheduledForward.PhoneNumber.Value)
                .ConfigureAwait(false);
        }

        public async Task ApplyForward(int teamId, int userPhoneNumberId, PerformContext context)
        {
            context.WriteLine(Debug, $"Looking up team {teamId} and userPhoneNumber {userPhoneNumberId}");
            var team = await db.Teams.FindAsync(teamId).ConfigureAwait(false);
            var userPhoneNumber = await db.UserPhoneNumbers.FindAsync(userPhoneNumberId).ConfigureAwait(false);

            if (team == null)
            {
                context.WriteLine(Warning, "Team not found, aborting.");
                return;
            }

            if (userPhoneNumber == null)
            {
                context.WriteLine(Warning, "UserPhoneNumber not found, aborting.");
                return;
            }

            context.WriteLine(Info, $"Found team {team.Name}, forward to {userPhoneNumber.Value}");

            await ForwardImpl(context, team.ComPort, userPhoneNumber.Value).ConfigureAwait(false);
        }

        public async Task ReadAllTeamStatus(PerformContext context)
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

        private async Task ForwardImpl(PerformContext context, string comPort, string phoneNumber)
        {
            await exclusiveTaskFactory.StartNew(() =>
            {
                context.WriteLine(Debug, "Forwarding ...");
                using (var helper = new ATHelper(comPort))
                {
                    helper.ForwardTo(phoneNumber);
                    context.WriteLine(Info, "Applied forward");

                    context.WriteLine(Debug, "Verifying ...");
                    var res = helper.GetForwardedPhoneNumber();
                    if (res != phoneNumber)
                    {
                        context.WriteLine(Error, $"Verification returned {res}, expected {phoneNumber}");
                        throw new Exception("Verification failed after applying forward");
                    }
                    context.WriteLine(Info, $"Verified forward to {phoneNumber}");
                }
            }).ConfigureAwait(false);
        }
    }
}