using System.Linq.Expressions;
using System.Reflection;

namespace SupportManager.Web.Infrastructure;

public static class QueryExtensions
{
    private static readonly NewMemberVisitor Visitor = new();

    public static TQueryable WithCtorMembers<TQueryable>(this TQueryable query) where TQueryable : IQueryable
    {
        return (TQueryable)query.Provider.CreateQuery(Visitor.Visit(query.Expression));
    }

    private class NewMemberVisitor : ExpressionVisitor
    {
        protected override Expression VisitNew(NewExpression node)
        {
            var visited = base.VisitNew(node) as NewExpression;

            if (visited is not { Members: null }) return visited!;

            var members = visited.Constructor!.GetParameters().Select(pi => node.Type.GetProperty(pi.Name!))
                .TakeWhile(pi => pi is not null).ToArray<MemberInfo>();

            return members.Length < node.Arguments.Count
                ? visited
                : Expression.New(visited.Constructor, visited.Arguments, members);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var visited = base.VisitMemberInit(node);

            if (visited is not MemberInitExpression { NewExpression.Members: { } members } mi) return visited;

            var set = members.ToHashSet();
            return node.Bindings.Any(binding => !set.Contains(binding.Member)) ? visited : mi.NewExpression;
        }
    }
}