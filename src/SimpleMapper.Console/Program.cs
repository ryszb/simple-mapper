using SimpleMapper.Core;

var mapper = new Mapper(new MappingRegistry(new MappingBuilder()));

var source = new Person { Name = "Alice", Age = 30, Address = new Address { City = "London", Street = "King’s Road" } };
var destination = mapper.Map<Person, PersonDto>(source);

Console.WriteLine(destination.Name); // Alice
Console.WriteLine(destination.Age);  // 30
Console.WriteLine(destination.Address!.City); // London
Console.WriteLine(destination.Address!.Street); // King’s Road

internal class PersonDto
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public AddressDto? Address { get; set; }
}

internal class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public Address? Address { get; set; }
}

internal class Address
{
    public string? City { get; set; }
    public string? Street { get; set; }
}

internal class AddressDto
{
    public string? City { get; set; }
    public string? Street { get; set; }
}