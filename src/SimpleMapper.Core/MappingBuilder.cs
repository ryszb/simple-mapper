using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core;

public class MappingBuilder : IMappingBuilder
{
    private readonly ConcurrentDictionary<(Type Source, Type Destination), LambdaExpression> _expressionCache = new();

    public Func<TSource, TDestination> BuildMapping<TSource, TDestination>()
    {
        var lambda = GetOrBuildMappingExpression(typeof(TSource), typeof(TDestination));

        return (Func<TSource, TDestination>)lambda.Compile();
    }

    private LambdaExpression GetOrBuildMappingExpression(Type sourceType, Type destinationType)
    {
        return _expressionCache.GetOrAdd((sourceType, destinationType), _ =>
        {
            var method = typeof(MappingBuilder)
                .GetMethod(nameof(BuildMappingExpression), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(sourceType, destinationType);

            return (LambdaExpression)method.Invoke(this, null)!;
        });
    }

    private LambdaExpression BuildMappingExpression<TSource, TDestination>()
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");
        var sourceProperties = GetSourceProperties(typeof(TSource))
            .ToDictionary(p => p.Name);

        var bindings = GetDestinationProperties(typeof(TDestination))
            .Select(destProp => CreateBinding(destProp, sourceParameter, sourceProperties))
            .ToList();

        var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);

        return Expression.Lambda<Func<TSource, TDestination>>(body, sourceParameter);
    }

    private MemberAssignment CreateBinding(
        PropertyInfo destProp,
        ParameterExpression sourceParameter,
        Dictionary<string, PropertyInfo> sourceProperties)
    {
        if (!sourceProperties.TryGetValue(destProp.Name, out var sourceProp))
        {
            return Expression.Bind(destProp, Expression.Default(destProp.PropertyType));
        }

        if (AreTypesCompatible(destProp.PropertyType, sourceProp.PropertyType))
        {
            return Expression.Bind(destProp, Expression.Property(sourceParameter, sourceProp));
        }

        if (IsComplexType(destProp.PropertyType))
        {
            var nestedLambda = GetOrBuildMappingExpression(sourceProp.PropertyType, destProp.PropertyType);

            if (!sourceProp.PropertyType.IsValueType || Nullable.GetUnderlyingType(sourceProp.PropertyType) != null)
            {
                var nullCheck = Expression.Condition(
                    Expression.Equal(Expression.Property(sourceParameter, sourceProp), Expression.Constant(null, sourceProp.PropertyType)),
                    Expression.Default(destProp.PropertyType),
                    Expression.Invoke(nestedLambda, Expression.Property(sourceParameter, sourceProp))
                );

                return Expression.Bind(destProp, nullCheck);
            }

            return Expression.Bind(destProp, Expression.Invoke(nestedLambda, Expression.Property(sourceParameter, sourceProp)));
        }

        return Expression.Bind(destProp, Expression.Default(destProp.PropertyType));
    }

    private static IEnumerable<PropertyInfo> GetSourceProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

    private static IEnumerable<PropertyInfo> GetDestinationProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

    private static bool AreTypesCompatible(Type destinationType, Type sourceType) =>
        destinationType.IsAssignableFrom(sourceType);

    private static bool IsComplexType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return !(underlyingType.IsPrimitive
                 || underlyingType.IsEnum
                 || underlyingType == typeof(string)
                 || underlyingType == typeof(decimal));
    }
}