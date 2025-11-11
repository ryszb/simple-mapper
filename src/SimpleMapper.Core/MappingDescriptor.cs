namespace SimpleMapper.Core;

public enum MappingKind
{
    Direct,
    Complex,
    Collection,
    Unsupported
}

public sealed class MappingDescriptor
{
    public MappingKind Kind { get; }
    public Type SourceType { get; }
    public Type DestinationType { get; }

    public MappingDescriptor(MappingKind kind, Type sourceType, Type destType)
    {
        Kind = kind;
        SourceType = sourceType;
        DestinationType = destType;
    }

    public static MappingDescriptor From(Type sourceType, Type destType)
    {
        if (AreTypesCompatible(destType, sourceType))
            return new(MappingKind.Direct, sourceType, destType);

        if (IsCollectionType(destType))
            return new(MappingKind.Collection, sourceType, destType);

        if (IsComplexType(destType))
            return new(MappingKind.Complex, sourceType, destType);

        return new(MappingKind.Unsupported, sourceType, destType);
    }

    private static bool AreTypesCompatible(Type dest, Type src) =>
        dest.IsAssignableFrom(src);

    private static bool IsComplexType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return !(underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(decimal));
    }

    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray ||
               (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type.IsGenericType);
    }
}