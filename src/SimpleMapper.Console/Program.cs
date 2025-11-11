using SimpleMapper.Core;
using SimpleMapper.Core.Binding;

var mapper = new Mapper(new MappingRegistry(new MappingBuilder(
[
    new DirectBindingStrategy(),
    new ComplexBindingStrategy(),
    new CollectionBindingStrategy(),
    new UnsupportedBindingStrategy()
])));

var source = new Person {
    Name = "Alice",
    Age = 30,
    Address = new() { City = "London", Street = "King’s Road" },
    PhoneNumbers = ["123-456-7890", "987-654-3210"],
    PreviousAddresses =
    [
        new() { City = "New York", Street = "5th Avenue" },
        new() { City = "San Francisco", Street = "Market Street" }
    ]
};
var destination = mapper.Map<Person, PersonDto>(source);

Console.WriteLine(destination.Name); // Alice
Console.WriteLine(destination.Age);  // 30
Console.WriteLine(destination.Address!.City); // London
Console.WriteLine(destination.Address!.Street); // King’s Road
Console.WriteLine(string.Join(", ", destination.PhoneNumbers));  // "123-456-7890", "987-654-3210"

foreach (var addr in destination.PreviousAddresses)
{
    Console.WriteLine($"{addr.City}, {addr.Street}");
    // New York, 5th Avenue
    // San Francisco, Market Street
}

internal class PersonDto
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public AddressDto? Address { get; set; }
    public List<string> PhoneNumbers { get; set; } = [];
    public List<AddressDto> PreviousAddresses { get; set; } = [];
}

internal class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public Address? Address { get; set; }
    public List<string> PhoneNumbers { get; set; } = [];
    public List<Address> PreviousAddresses { get; set; } = [];
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