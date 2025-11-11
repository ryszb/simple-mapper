using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core.Binding;

public class UnsupportedBindingStrategy : IBindingStrategy
{
    public MappingKind Kind => MappingKind.Unsupported;

    public MemberAssignment BuildAssignment(
        PropertyInfo destProp,
        PropertyInfo sourceProp,
        ParameterExpression sourceParameter,
        MappingBuilder builder) 
            => throw new UnsupportedMappingException(sourceProp.PropertyType, destProp.PropertyType);
}