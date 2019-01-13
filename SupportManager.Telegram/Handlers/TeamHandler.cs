using System;
using System.Linq;
using System.Threading.Tasks;
using SupportManager.Api.Teams;
using SupportManager.Telegram.Infrastructure;

namespace SupportManager.Telegram.Handlers
{
    internal class TeamHandler
    {
        [Command("get_schedule")]
        public async Task GetSchedule(Interaction interaction, ISupportManagerApi api)
        {
            var schedule = await api.GetTeamSchedule(await interaction.ReadTeam("For which *team* do you want to view the schedule?"));
            var reply = schedule.Any()
                ? string.Join("\n\n", schedule.Select(FormatRegistration))
                : "Nothing scheduled";
            await interaction.Write(reply);
        }

        [Command("get_status")]
        public async Task GetStatus(Interaction interaction, ISupportManagerApi api)
        {
            var status = await api.GetTeamStatus(await interaction.ReadTeam("For which *team* do you want to view the status?"));
            var scheduled = status.ScheduledForward == null
                ? "Nothing scheduled"
                : FormatRegistration(status.ScheduledForward);
            await interaction.Write($"Current status:\n{FormatRegistration(status.CurrentForward)}\n\n{scheduled}");
        }

        [Command("schedule")]
        public async Task Schedule(ISupportManagerApi api, Interaction interaction)
        {
            var teamId = await interaction.ReadTeam("For which *team* do you want to schedule?");
            var date = await interaction.ReadDate("Which *day* do you want to schedule?");
            var minTime = date.Equals(DateTime.Today) ? DateTime.Now.TimeOfDay.Nullable() : null;
            var timeOfDay = await interaction.ReadTime("What *time* do you want to schedule?", minTime);
            var phoneNumberId = await interaction.ReadPhoneNumber("*Who* do you want to forward to?", teamId);

            await api.ScheduleForward(new ScheduleForward
            {
                PhoneNumberId = phoneNumberId,
                TeamId = teamId,
                When = DateTime.SpecifyKind(date.Add(timeOfDay), DateTimeKind.Local)
            });

            await interaction.Write("Forward scheduled!");
        }

        [Command("forward")]
        public async Task Forward(ISupportManagerApi api, Interaction interaction)
        {
            var teamId = await interaction.ReadTeam("For which *team* do you want to forward?");
            var phoneNumberId = await interaction.ReadPhoneNumber("*Who* do you want to forward to?", teamId);

            await api.SetForward(new SetForward {PhoneNumberId = phoneNumberId, TeamId = teamId});
            await interaction.Write("Queued redirect, please allow up to a minute for the status to update.");
        }

        [Command("delete")]
        public async Task Delete(ISupportManagerApi api, Interaction interaction)
        {
            var teamId = await interaction.ReadTeam("For which *team* do you want to delete an entry?");
            var forwards = await api.GetTeamSchedule(teamId);
            var id = await interaction.ReadOption("Which entry do you want to delete?",
                forwards.OrderBy(fwd => fwd.When), fwd => $"{fwd.When.ToHumanReadable()} {fwd.User.DisplayName}",
                fwd => fwd.Id);

            var selected = forwards.Single(x => x.Id == id);

            await api.DeleteForward(id);
            await interaction.Write(
                $"Deleted schedule entry *{selected.When.ToHumanReadable()}* {selected.User.DisplayName}.");
        }

        private static string FormatRegistration(ForwardRegistration registration)
        {
            return $"*{registration.When.ToHumanReadable()}*:\n{registration.User.DisplayName}\n{registration.PhoneNumber.Value}";
        }
    }
}
