using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SupportManager.Backup.Internal
{
    public static class WithMemberExtensions
    {
        public static Expression<Func<TSource, TResult>> WithMembers<TSource, TResult>(
            this Expression<Func<TSource, TResult>> expression)
        {
            var body = expression.Body as NewExpression ??
                throw new ArgumentException($"Expression should be a {nameof(NewExpression)}.", nameof(expression));

            return expression.Update(body.WithMembers(), expression.Parameters);
        }

        public static NewExpression WithMembers(this NewExpression node)
        {
            if (node.Members != null) return node;

            var members = new MemberInfo[node.Arguments.Count];
            foreach (var param in node.Constructor.GetParameters())
            {
                var member = GetMember(node.Type, param.Name.UcFirst()) ??
                    throw new Exception($"No member found for constructor parameter {param.Name}.");

                members[param.Position] = member;
            }

            return Expression.New(node.Constructor, node.Arguments, members);
        }

        public static IQueryable<T> WithMembers<T>(this IQueryable<T> queryable)
        {
            return queryable.Provider.CreateQuery<T>(new WithMembersVisitor().Visit(queryable.Expression));
        }

        private static MemberInfo GetMember(Type type, string name)
        {
            var members = type.GetMember(name);
            return members.Length == 0 ? null : members[0];
        }

        private static string UcFirst(this string input)
        {
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private class WithMembersVisitor : ExpressionVisitor
        {
            protected override Expression VisitNew(NewExpression node)
            {
                return base.VisitNew(node.WithMembers());
            }
        }
    }
}
