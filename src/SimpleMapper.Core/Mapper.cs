namespace SimpleMapper.Core;

public class Mapper : IMapper
{
    private readonly MappingRegistry _registry;

    public Mapper(MappingRegistry registry) => _registry = registry;

    public TDestination Map<TSource, TDestination>(TSource? source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        
        var mapFunc = _registry.GetOrAdd<TSource, TDestination>();
        return mapFunc(source);
    }
}