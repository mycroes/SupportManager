namespace SupportManager.Web.Infrastructure;

/// <summary>
/// Marks a team page as read-only, granting access to observers that are not a member of the team.
/// </summary>
/// <remarks>
/// Observers are only permitted to issue GET requests to pages with this attribute. Only apply this to pages which
/// don't modify any data on GET.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class AllowObserverAttribute : Attribute
{
}
