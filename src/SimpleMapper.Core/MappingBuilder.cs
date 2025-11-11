using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using SimpleMapper.Core.Binding;

namespace SimpleMapper.Core;

public class MappingBuilder : IMappingBuilder
{
    private readonly IReadOnlyDictionary<MappingKind, IBindingStrategy> _strategies;
    private readonly ConcurrentDictionary<(Type Source, Type Destination), LambdaExpression> _expressionCache = new();

    public MappingBuilder(IEnumerable<IBindingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Kind);
    }

    public Func<TSource, TDestination> BuildMapping<TSource, TDestination>()
    {
        var lambda = GetOrBuildMappingExpression(typeof(TSource), typeof(TDestination));

        return (Func<TSource, TDestination>)lambda.Compile();
    }

    internal LambdaExpression GetOrBuildMappingExpression(Type sourceType, Type destinationType)
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

        var descriptor = MappingDescriptor.From(sourceProp.PropertyType, destProp.PropertyType);

        if (!_strategies.TryGetValue(descriptor.Kind, out var strategy))
        {
            throw new BindingStrategyNotFoundException(descriptor.SourceType, descriptor.DestinationType);
        }

        return strategy.BuildAssignment(destProp, sourceProp, sourceParameter, this);
    }
    
    private static IEnumerable<PropertyInfo> GetSourceProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

    private static IEnumerable<PropertyInfo> GetDestinationProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);
}