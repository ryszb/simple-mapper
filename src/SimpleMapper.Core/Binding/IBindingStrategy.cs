using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core.Binding;

public interface IBindingStrategy
{
    MappingKind Kind { get; }
    
    MemberAssignment BuildAssignment(
        PropertyInfo destProp,
        PropertyInfo sourceProp,
        ParameterExpression sourceParameter,
        MappingBuilder builder);
}