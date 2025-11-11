using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core.Binding;

public sealed class DirectBindingStrategy : IBindingStrategy
{
    public MappingKind Kind => MappingKind.Direct;

    public MemberAssignment BuildAssignment(
        PropertyInfo destProp,
        PropertyInfo sourceProp,
        ParameterExpression sourceParameter,
        MappingBuilder builder) 
            => Expression.Bind(destProp, Expression.Property(sourceParameter, sourceProp));
}