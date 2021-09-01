using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SupportManager.Backup.Internal;
using SupportManager.Backup.Schema;

namespace SupportManager.Backup
{
    internal class Exporter
    {
        private readonly DAL.SupportManagerContext db;
        private readonly string fileName;
        private readonly IProgress<string> progress;

        public Exporter(DAL.SupportManagerContext db, string fileName, IProgress<string> progress)
        {
            this.db = db;
            this.fileName = fileName;
            this.progress = progress;
        }

        public async Task Export()
        {
            FileStream file;
            try
            {
                file = File.OpenWrite(fileName);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to open file '{fileName}': {e.Message}.");
            }

            progress.Report("Loading teams...");
            var teams = await db.Teams.Select(t => new SupportTeam(t.Id, t.Name, t.ConnectionString)).WithMembers()
                .ToListAsync();

            progress.Report("Loading users...");
            var users = await db.Users.Select(u => new User(u.Id, u.DisplayName, u.Login,
                    u.EmailAddresses.Select(e => new UserEmailAddress(e.Id, e.Value, e.IsVerified)).ToList(),
                    u.PhoneNumbers.Select(p => new UserPhoneNumber(p.Id, p.Label, p.Value, p.IsVerified)).ToList(),
                    u.ApiKeys.Select(k => new ApiKey(k.Value, k.CallbackUrl)).ToList(), u.PrimaryEmailAddress.Id,
                    u.PrimaryPhoneNumber.Id,
                    u.Memberships.Select(m => new TeamMember(m.TeamId, m.IsAdministrator)).ToList())).WithMembers()
                .ToListAsync();

            progress.Report("Loading schedule...");
            var schedule = await db.ScheduledForwards
                .Select(f => new ScheduledForward(f.TeamId, f.When, f.PhoneNumberId)).WithMembers().ToListAsync();

            progress.Report("Loading history...");
            var history = await db.ForwardingStates.Select(f =>
                    new ForwardingState(f.TeamId, f.When, f.DetectedPhoneNumberId, f.RawPhoneNumber)).WithMembers()
                .ToListAsync();

            progress.Report($"Writing data to '{fileName}'...");
            var backupData = new BackupData(teams, users, schedule, history);
            await JsonSerializer.SerializeAsync(file, backupData);
            file.Close();
        }
    }
}
