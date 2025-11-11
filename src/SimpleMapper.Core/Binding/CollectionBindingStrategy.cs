using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Core.Binding;

public sealed class CollectionBindingStrategy : IBindingStrategy
{
    public MappingKind Kind => MappingKind.Collection;

    public MemberAssignment BuildAssignment(
        PropertyInfo destProp,
        PropertyInfo sourceProp,
        ParameterExpression sourceParameter,
        MappingBuilder builder)
    {
        var sourcePropertyExpr = Expression.Property(sourceParameter, sourceProp);

        var sourceElementType = GetElementType(sourceProp.PropertyType);
        var destElementType = GetElementType(destProp.PropertyType);

        var mapElementLambda = builder.GetOrBuildMappingExpression(sourceElementType, destElementType);

        var selectMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == nameof(Enumerable.Select) && m.GetParameters().Length == 2)
            .MakeGenericMethod(sourceElementType, destElementType);

        var projected = Expression.Call(selectMethod, sourcePropertyExpr, mapElementLambda);

        Expression toCollectionCall;
        Expression emptyCollection;

        if (destProp.PropertyType.IsArray)
        {
            (toCollectionCall, emptyCollection) = BuildArrayExpressions(projected, destElementType);
        }
        else
        {
            (toCollectionCall, emptyCollection) = BuildListExpressions(projected, destElementType);
        }

        var body = Expression.Condition(
            Expression.NotEqual(sourcePropertyExpr, Expression.Constant(null, sourceProp.PropertyType)),
            toCollectionCall,
            emptyCollection
        );

        return Expression.Bind(destProp, body);
    }

    private static Type GetElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType()!;
        if (type.IsGenericType)
            return type.GetGenericArguments()[0];

        return typeof(object);
    }

    private (Expression mapped, Expression empty) BuildArrayExpressions(Expression projected, Type destElementType)
    {
        var toArrayMethod = typeof(Enumerable)
            .GetMethod(nameof(Enumerable.ToArray))!
            .MakeGenericMethod(destElementType);
        
        var mapped = Expression.Call(toArrayMethod, projected);

        var emptyArrayMethod = typeof(Array)
            .GetMethod(nameof(Array.Empty))!
            .MakeGenericMethod(destElementType);
        var empty = Expression.Call(emptyArrayMethod);

        return (mapped, empty);
    }

    private (Expression mapped, Expression empty) BuildListExpressions(Expression projected, Type destElementType)
    {
        var toListMethod = typeof(Enumerable)
            .GetMethod(nameof(Enumerable.ToList))!
            .MakeGenericMethod(destElementType);

        var mapped = Expression.Call(toListMethod, projected);
        var empty = Expression.New(typeof(List<>).MakeGenericType(destElementType));

        return (mapped, empty);
    }
}