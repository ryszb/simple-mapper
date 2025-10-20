using SimpleMapper.Core;

var mapper = new Mapper(new MappingRegistry(new MappingBuilder()));

var source = new Person { Name = "Alice", Age = 30 };
var destination = mapper.Map<Person, PersonDto>(source);

Console.WriteLine(destination.Name); // Alice
Console.WriteLine(destination.Age);  // 30

internal class PersonDto
{
    public string? Name { get; set; }
    public int Age { get; set; }
}

internal class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
}