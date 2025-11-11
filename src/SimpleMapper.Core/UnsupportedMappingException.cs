namespace SimpleMapper.Core;

public class UnsupportedMappingException : Exception
{
    public Type SourceType { get; }
    
    public Type DestinationType { get; }

    public UnsupportedMappingException(Type sourceType, Type destinationType)
        : base($"Mapping from '{sourceType.Name}' to '{destinationType.Name}' is not supported.")
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}