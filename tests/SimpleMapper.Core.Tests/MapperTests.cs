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
        public Address? Address { get; set; }
    }

    public class PersonDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public AddressDto? Address { get; set; }
    }

    public class Address
    {
        public string? City { get; set; }
        public string? Street { get; set; }
    }

    public class AddressDto
    {
        public string? City { get; set; }
        public string? Street { get; set; }
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
    public void MapsNestedProperties()
    {
        var source = new Person
        {
            Name = "Bob",
            Age = 40,
            Address = new Address { City = "Paris", Street = "Rue de Rivoli" }
        };

        var result = _mapper.Map<Person, PersonDto>(source);

        Assert.Equal("Bob", result.Name);
        Assert.Equal(40, result.Age);
        Assert.NotNull(result.Address);
        Assert.Equal("Paris", result.Address!.City);
        Assert.Equal("Rue de Rivoli", result.Address.Street);
    }

    [Fact]
    public void NestedPropertyCanBeNull()
    {
        var source = new Person
        {
            Name = "Charlie",
            Age = 25,
            Address = null
        };

        var result = _mapper.Map<Person, PersonDto>(source);

        Assert.Equal("Charlie", result.Name);
        Assert.Equal(25, result.Age);
        Assert.Null(result.Address);
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