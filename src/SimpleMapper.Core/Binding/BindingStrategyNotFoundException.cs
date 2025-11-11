namespace SimpleMapper.Core.Binding;

public class BindingStrategyNotFoundException : Exception
{
    public Type SourceType { get; }
    public Type DestinationType { get; }

    public BindingStrategyNotFoundException(Type sourceType, Type destinationType)
        : base($"No binding strategy found for mapping {sourceType.Name} → {destinationType.Name}.")
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}
