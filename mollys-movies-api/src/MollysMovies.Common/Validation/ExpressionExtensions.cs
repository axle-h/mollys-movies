using System.Linq.Expressions;
using System.Reflection;

namespace MollysMovies.Common.Validation;

public static class ExpressionExtensions
{
    public static string GetMemberName<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression) =>
        GetMember(expression).Name;

    private static MemberInfo GetMember<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression) =>
        expression.Body switch
        {
            MemberExpression member => member.Member,
            UnaryExpression {Operand: MemberExpression operand} => operand.Member,
            _ => throw new ArgumentException("Not a member or unary expression: " + expression, nameof(expression))
        };
}