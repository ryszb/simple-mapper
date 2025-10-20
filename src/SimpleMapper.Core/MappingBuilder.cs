using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core;

public class MappingBuilder : IMappingBuilder
{
    public Func<TSource, TDestination> BuildMapping<TSource, TDestination>()
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");

        var sourceProperties = GetSourceProperties(typeof(TSource))
            .ToDictionary(p => p.Name);

        var bindings = GetDestinationProperties(typeof(TDestination))
            .Select(destProp => CreateBinding(destProp, sourceParameter, sourceProperties));

        var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
        var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParameter);

        return lambda.Compile();
    }

    private static IEnumerable<PropertyInfo> GetSourceProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

    private static IEnumerable<PropertyInfo> GetDestinationProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

    private static MemberAssignment CreateBinding(
        PropertyInfo destProp,
        ParameterExpression sourceParameter,
        Dictionary<string, PropertyInfo> sourceProperties)
    {
        Expression valueExpression =
            sourceProperties.TryGetValue(destProp.Name, out var sourceProp) &&
            AreTypesCompatible(destProp.PropertyType, sourceProp.PropertyType)
                ? Expression.Property(sourceParameter, sourceProp)
                : Expression.Default(destProp.PropertyType);

        return Expression.Bind(destProp, valueExpression);
    }

    private static bool AreTypesCompatible(Type destinationType, Type sourceType) =>
        destinationType.IsAssignableFrom(sourceType);
}