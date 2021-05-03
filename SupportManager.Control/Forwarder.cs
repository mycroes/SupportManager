using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Control
{
    public class Forwarder : IForwarder
    {
        private readonly SupportManagerContext db;

        private static readonly TaskFactory ExclusiveTaskFactory =
            new TaskFactory(new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler);

        private static readonly ConsoleTextColor Debug = ConsoleTextColor.Green;
        private static readonly ConsoleTextColor Info = ConsoleTextColor.White;
        private static readonly ConsoleTextColor Warning = ConsoleTextColor.Yellow;
        private static readonly ConsoleTextColor Error = ConsoleTextColor.Red;

        public Forwarder(SupportManagerContext db)
        {
            this.db = db;
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

            await ForwardImpl(context, scheduledForward.Team.ConnectionString, scheduledForward.PhoneNumber.Value)
                .ConfigureAwait(false);
            await ReadTeamStatus(scheduledForward.Team, context);
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

            await ForwardImpl(context, team.ConnectionString, userPhoneNumber.Value).ConfigureAwait(false);
            await ReadTeamStatus(team, context);
        }

        public async Task ReadAllTeamStatus(PerformContext context)
        {
            foreach (var team in db.Teams.ToList())
            {
                await ReadTeamStatus(team, context);
            }
        }

        private async Task ReadTeamStatus(SupportTeam team, PerformContext context)
        {
            var number = await ExclusiveTaskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(team.ConnectionString)) return helper.GetForwardedPhoneNumber();
            });

            context.WriteLine($"Team {team.Name} is forwarding to '{number}'.");

            var state = await db.ForwardingStates.OrderByDescending(s => s.When).FirstOrDefaultAsync();
            if (state?.RawPhoneNumber == number)
            {
                context.WriteLine("No state change, completed check.");
                return;
            }

            context.WriteLine($"Change detected from '{state?.RawPhoneNumber}' to '{number}'.");
            var newState = new ForwardingState
            {
                Team = team,
                When = DateTimeOffset.Now,
                RawPhoneNumber = number,
                DetectedPhoneNumber = db.UserPhoneNumbers.FirstOrDefault(p => p.Value == number)
            };

            db.ForwardingStates.Add(newState);
            await db.SaveChangesAsync();

            var prevStateId = state?.Id;
            await db.Teams.Where(t => t.Id == team.Id).SelectMany(t => t.Members).Select(m => m.User)
                .SelectMany(u => u.ApiKeys).Where(a => a.CallbackUrl != null).ForEachAsync(apiKey =>
                {
                    context.WriteLine($"Scheduling notification to {apiKey.CallbackUrl}.");
                    BackgroundJob.Enqueue<IPublisher>(p =>
                        p.NotifyStateChange(prevStateId, newState.Id, apiKey.CallbackUrl, null));
                });
        }

        private async Task ForwardImpl(PerformContext context, string connectionString, string phoneNumber)
        {
            await ExclusiveTaskFactory.StartNew(() =>
            {
                context.WriteLine(Debug, "Forwarding ...");
                using (var helper = new ATHelper(connectionString))
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