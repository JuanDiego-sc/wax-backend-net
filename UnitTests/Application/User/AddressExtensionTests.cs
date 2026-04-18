using Application.User.Extensions;
using Domain.Entities;

namespace UnitTests.Application.User;

public class AddressExtensionTests
{
    [Fact]
    public void ToDto_MapsAllBillingAddressFields()
    {
        var billingAddress = new BillingAddress
        {
            Name = "John Doe",
            Line1 = "Main Street",
            Line2 = "Apt 5",
            City = "Quito",
            State = "Pichincha",
            PostalCode = "170101",
            Country = "EC"
        };

        var dto = billingAddress.ToDto();

        dto.Name.Should().Be("John Doe");
        dto.Line1.Should().Be("Main Street");
        dto.Line2.Should().Be("Apt 5");
        dto.City.Should().Be("Quito");
        dto.State.Should().Be("Pichincha");
        dto.PostalCode.Should().Be("170101");
        dto.Country.Should().Be("EC");
    }
}
