using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MolliesMovies.Common.Validation
{
    internal static class ExpressionExtensions
    {
        public static string GetMemberName<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression) =>
            GetMember(expression).Name;

        public static MemberInfo GetMember<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression)
        {
            switch (expression.Body)
            {
                case MemberExpression member:
                    return member.Member;

                case UnaryExpression unary when unary.Operand is MemberExpression operand:
                    return operand.Member;

                default:
                    throw new ArgumentException("Not a member or unary expression: " + expression, nameof(expression));
            }
        }
    }
}