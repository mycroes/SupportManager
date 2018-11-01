namespace SupportManager.Telegram.Infrastructure
{
    internal static class NullableExtensions
    {
        public static T? Nullable<T>(this T value) where T : struct => value;
    }
}
