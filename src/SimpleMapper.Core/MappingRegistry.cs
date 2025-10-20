using System.Collections.Concurrent;

namespace SimpleMapper.Core;

public class MappingRegistry
{
    private readonly IMappingBuilder _builder;
    private readonly ConcurrentDictionary<(Type Source, Type Destination), object> _cache = new();

    public MappingRegistry(IMappingBuilder builder) => _builder = builder;

    public Func<TSource, TDestination> GetOrAdd<TSource, TDestination>()
    {
        var key = (typeof(TSource), typeof(TDestination));

        var mapping = (Func<TSource, TDestination>)_cache.GetOrAdd(
            key,
            _ => _builder.BuildMapping<TSource, TDestination>());

        return mapping;
    }
}