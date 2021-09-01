using System;

namespace SupportManager.Backup.Schema
{
    internal record ScheduledForward(int TeamId, DateTimeOffset When, int UserPhoneNumberId);
}