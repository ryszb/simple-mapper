namespace SimpleMapper.Core;

public interface IMappingBuilder
{
    Func<TSource, TDestination> BuildMapping<TSource, TDestination>();
}