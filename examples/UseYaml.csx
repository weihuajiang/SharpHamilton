#r "nuget:YamlDotNet/13.7.1"
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
}
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public float HeightInInches { get; set; }
    public Dictionary<string, Address> Addresses { get; set; }

}
var person = new Person
{
    Name = "Abe Lincoln",
    Age = 25,
    HeightInInches = 6f + 4f / 12f,
    Addresses = new Dictionary<string, Address>{
                { "home", new  Address() {
                        Street = "2720  Sundown Lane",
                        City = "Kentucketsville",
                        State = "Calousiyorkida",
                        Zip = "99978",
                    }},
                { "work", new  Address() {
                        Street = "1600 Pennsylvania Avenue NW",
                        City = "Washington",
                        State = "District of Columbia",
                        Zip = "20500",
                    }},
            }
};

var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
var yaml = serializer.Serialize(person);
System.Console.WriteLine(yaml);

var yml = @"
name: George Washington
age: 89
height_in_inches: 5.75
addresses:
  home:
    street: 400 Mockingbird Lane
    city: Louaryland
    state: Hawidaho
    zip: 99970
";
var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

var p = deserializer.Deserialize<Person>(yml);
var h = p.Addresses["home"];
System.Console.WriteLine($"{p.Name} is {p.Age} years old and lives at {h.Street} in {h.City}, {h.State}.");