using Moq;

namespace SimpleMapper.Core.Tests;

public class MapperTests
{
    private readonly MappingBuilder _builder;
    private readonly MappingRegistry _registry;
    private readonly Mapper _mapper;

    public MapperTests()
    {
        _builder = new MappingBuilder();
        _registry = new MappingRegistry(_builder);
        _mapper = new Mapper(_registry);
    }

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class PersonDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void ThrowsOnNullSource()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.Map<Person, PersonDto>(null));
    }
        
    [Fact]
    public void MapsSimpleProperties()
    {
        var source = new Person { Name = "Alice", Age = 30 };
        var result = _mapper.Map<Person, PersonDto>(source);

        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void UsesDefaultValueForMissingProperties()
    {
        var source = new Person { Name = "Alice" };
        var result = _mapper.Map<Person, PersonDto>(source);

        Assert.Equal("Alice", result.Name);
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public void ReusesCachedDelegate()
    {
        var builderMock = new Mock<IMappingBuilder>();

        builderMock
            .Setup(b => b.BuildMapping<Person, PersonDto>())
            .Returns(new Func<Person, PersonDto>(s => new PersonDto()));

        var registry = new MappingRegistry(builderMock.Object);
        var mapper = new Mapper(registry);

        var result1 = mapper.Map<Person, PersonDto>(new Person());
        var result2 = mapper.Map<Person, PersonDto>(new Person());

        builderMock.Verify(b => b.BuildMapping<Person, PersonDto>(), Times.Once);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
    }
}