using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SupportManager.Backup.Schema;
using SupportManager.DAL;

namespace SupportManager.Backup
{
    internal class Importer
    {
        private readonly SupportManagerContext db;
        private readonly string fileName;
        private readonly IProgress<string> progress;

        public Importer(SupportManagerContext db, string fileName, IProgress<string> progress)
        {
            this.db = db;
            this.fileName = fileName;
            this.progress = progress;
        }

        public async Task Import()
        {
            var backupData = await ReadFile();

            using var tx = db.Database.BeginTransaction();

            var teamMap = await ImportTeams(backupData);
            var phoneNumberMap = await ImportUsers(backupData, teamMap);

            await ImportHistory(backupData, teamMap, phoneNumberMap);
            await ImportSchedule(backupData, teamMap, phoneNumberMap);

            await db.SaveChangesAsync();
            tx.Commit();
        }

        private async Task ImportHistory(BackupData backupData, IDictionary<int, DAL.SupportTeam> teamMap, IDictionary<int, DAL.UserPhoneNumber> phoneNumberMap)
        {
            var teamHist = await db.ForwardingStates.GroupBy(s => s.TeamId,
                (id, states) => new
                {
                    TeamId = id, Min = states.Select(s => s.When).Min(), Max = states.Select(s => s.When).Max()
                }).ToDictionaryAsync(x => x.TeamId);

            foreach (var entry in backupData.History)
            {
                var team = teamMap[entry.TeamId];
                if (teamHist.TryGetValue(team.Id, out var hist))
                {
                    // Skip history update within known history
                    if (entry.When >= hist.Min && entry.When <= hist.Max) continue;
                }

                DAL.UserPhoneNumber detected = null;
                if (entry.DetectedPhoneNumberId.HasValue)
                {
                    detected = phoneNumberMap[entry.DetectedPhoneNumberId.Value];
                }

                db.ForwardingStates.Add(new()
                {
                    Team = team,
                    RawPhoneNumber = entry.RawPhoneNumber,
                    DetectedPhoneNumber = detected,
                    When = entry.When
                });
            }
        }

        private async Task ImportSchedule(BackupData backupData, IDictionary<int, DAL.SupportTeam> teamMap,
            IDictionary<int, DAL.UserPhoneNumber> phoneNumberMap)
        {
            var teamLatest = await db.ScheduledForwards.GroupBy(s => s.TeamId,
                (id, states) => new
                {
                    TeamId = id, Latest = states.Select(s => s.When).Max()
                }).ToDictionaryAsync(x => x.TeamId, x => x.Latest);

            foreach (var entry in backupData.Schedule)
            {
                var team = teamMap[entry.TeamId];
                if (teamLatest.TryGetValue(team.Id, out var latest))
                {
                    // Skip schedule update within known schedule
                    if (entry.When <= latest) continue;
                }

                db.ScheduledForwards.Add(new()
                {
                    Team = team, PhoneNumber = phoneNumberMap[entry.UserPhoneNumberId], When = entry.When
                });
            }
        }

        private async Task<IDictionary<int, DAL.SupportTeam>> ImportTeams(BackupData backupData)
        {
            Dictionary<int, DAL.SupportTeam> teamMap = new();
            foreach (var (id, name, connectionString) in backupData.Teams)
            {
                var dbTeam = await db.Teams.Where(t => t.Name == name).FirstOrDefaultAsync() ??
                    db.Teams.Add(new() {Name = name, ConnectionString = connectionString});

                teamMap.Add(id, dbTeam);

                progress.Report(dbTeam.Id == 0
                    ? $"Created Team '{name}'"
                    : $"Found existing team '{name}', leaving team settings as-is");
            }

            return teamMap;
        }

        private async Task<IDictionary<int, DAL.UserPhoneNumber>> ImportUsers(BackupData backupData,
            IDictionary<int, DAL.SupportTeam> teamMap)
        {
            Dictionary<int, DAL.UserPhoneNumber> phoneNumberMap = new();
            foreach (var user in backupData.Users)
            {
                var target = await db.Users.Include(u => u.ApiKeys).Include(u => u.PhoneNumbers)
                    .Include(u => u.EmailAddresses).Include(u => u.Memberships).Where(u => u.Login == user.Login)
                    .Include(u => u.PrimaryPhoneNumber).Include(u => u.PrimaryEmailAddress).FirstOrDefaultAsync();

                if (target != null)
                {
                    progress.Report(
                        $"Found existing user {target.DisplayName} with login {target.Login}, leaving user as-is");
                }
                else
                {
                    target = new()
                    {
                        Login = user.Login,
                        DisplayName = user.DisplayName,
                        Memberships = new List<DAL.TeamMember>(),
                        ApiKeys = new List<DAL.ApiKey>(),
                        EmailAddresses = new List<DAL.UserEmailAddress>(),
                        PhoneNumbers = new List<DAL.UserPhoneNumber>()
                    };

                    db.Users.Add(target);
                    await db.SaveChangesAsync();
                }

                foreach (var (teamId, isAdministrator) in user.Memberships)
                {
                    var team = teamMap[teamId];
                    if (target.Memberships.Any(x => x.TeamId == team.Id)) continue;

                    target.Memberships.Add(new() {Team = team, IsAdministrator = isAdministrator});
                }

                foreach (var phoneNumber in user.PhoneNumbers)
                {
                    var existing = target.PhoneNumbers.FirstOrDefault(x => x.Value == phoneNumber.Value);
                    if (existing != null)
                    {
                        phoneNumberMap.Add(phoneNumber.Id, existing);
                        continue;
                    }

                    var userPhoneNumber = new DAL.UserPhoneNumber
                    {
                        IsVerified = phoneNumber.IsVerified,
                        Label = phoneNumber.Label,
                        Value = phoneNumber.Value,
                        User = target
                    };
                    target.PhoneNumbers.Add(userPhoneNumber);
                    phoneNumberMap.Add(phoneNumber.Id, userPhoneNumber);

                    if (user.PrimaryPhoneNumberId == phoneNumber.Id && target.PrimaryPhoneNumber == null)
                    {
                        target.PrimaryPhoneNumber = userPhoneNumber;
                    }
                }

                foreach (var emailAddress in user.EmailAddresses)
                {
                    if (target.EmailAddresses.Any(x => x.Value == emailAddress.Value)) continue;

                    var userEmailAddress = new DAL.UserEmailAddress
                    {
                        IsVerified = emailAddress.IsVerified, Value = emailAddress.Value
                    };
                    target.EmailAddresses.Add(userEmailAddress);

                    if (user.PrimaryEmailAddressId == emailAddress.Id && target.PrimaryEmailAddress == null)
                    {
                        target.PrimaryEmailAddress = userEmailAddress;
                    }
                }

                foreach (var apiKey in user.ApiKeys)
                {
                    if (target.ApiKeys.Any(x => x.CallbackUrl == apiKey.CallbackUrl)) continue;

                    target.ApiKeys.Add(new() {Value = apiKey.Value, CallbackUrl = apiKey.CallbackUrl});
                }
            }

            return phoneNumberMap;
        }

        private async Task<BackupData> ReadFile()
        {
            FileStream file;
            try
            {
                file = File.OpenRead(fileName);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to open file '{fileName}': {e.Message}.");
            }

            BackupData? backupData;
            progress.Report($"Loading data from '{fileName}'...");
            try
            {
                backupData = await JsonSerializer.DeserializeAsync<BackupData>(file);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load data from '{fileName}': {e.Message}");
            }

            if (backupData == null)
            {
                throw new Exception("No data found to import");
            }

            return backupData;
        }
    }
}
