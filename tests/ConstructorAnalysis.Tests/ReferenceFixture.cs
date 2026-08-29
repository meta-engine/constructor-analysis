namespace ConstructorAnalysis.Tests;

public class ReferenceFixture
{
    public required AddressFixture Address { get; set; }
    public required IContactFixture Contact { get; set; }
    public ReferenceFixture(AddressFixture address, IContactFixture contact)
    {
        Address = address;
        Contact = contact;
    }
}
