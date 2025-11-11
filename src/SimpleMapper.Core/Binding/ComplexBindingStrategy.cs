using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core.Binding;

public sealed class ComplexBindingStrategy : IBindingStrategy
{
    public MappingKind Kind => MappingKind.Complex;

    public MemberAssignment BuildAssignment(
        PropertyInfo destProp,
        PropertyInfo sourceProp,
        ParameterExpression sourceParameter,
        MappingBuilder builder)
    {
        var nestedLambda = builder.GetOrBuildMappingExpression(sourceProp.PropertyType, destProp.PropertyType);
        var sourcePropertyExpr = Expression.Property(sourceParameter, sourceProp);

        if (!sourceProp.PropertyType.IsValueType ||
            Nullable.GetUnderlyingType(sourceProp.PropertyType) != null)
        {
            var nullCheck = Expression.Condition(
                Expression.Equal(sourcePropertyExpr, Expression.Constant(null, sourceProp.PropertyType)),
                Expression.Default(destProp.PropertyType),
                Expression.Invoke(nestedLambda, sourcePropertyExpr));

            return Expression.Bind(destProp, nullCheck);
        }

        return Expression.Bind(destProp, Expression.Invoke(nestedLambda, sourcePropertyExpr));
    }
}
