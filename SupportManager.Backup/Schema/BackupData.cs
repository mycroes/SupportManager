using System.Collections.Generic;

namespace SupportManager.Backup.Schema
{
    internal record BackupData(List<SupportTeam> Teams, List<User> Users, List<ScheduledForward> Schedule,
        List<ForwardingState> History);
}