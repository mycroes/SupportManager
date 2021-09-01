using System;

namespace SupportManager.Backup.Schema
{
    internal record ForwardingState(int TeamId, DateTimeOffset When, int? DetectedPhoneNumberId, string RawPhoneNumber);
}