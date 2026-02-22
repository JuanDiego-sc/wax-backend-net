using System;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Address
{
    [System.Text.Json.Serialization.JsonIgnore]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }

    [JsonPropertyName("postal_code")]
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
}
