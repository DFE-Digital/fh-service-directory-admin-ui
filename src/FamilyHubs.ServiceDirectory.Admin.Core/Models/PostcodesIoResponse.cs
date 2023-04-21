
// ReSharper disable UnusedMember.Global

using Newtonsoft.Json;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Models;
public class PostcodesIoResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("error")]
    public int Error { get; set; }

    [JsonProperty("result")]
    public PostcodeInfo Result { get; set; } = default!;
}

public class PostcodeInfo
{
    [JsonProperty("postcode")]
    public string Postcode { get; set; } = default!;

    public string AdminArea => string.Equals(Codes.AdminCounty, "E99999999", StringComparison.InvariantCultureIgnoreCase) ? Codes.AdminDistrict : Codes.AdminCounty;

    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("outcode")]
    public string? OutCode { get; set; }

    [JsonProperty("country")]
    public string? Country { get; set; }

    [JsonProperty("codes")]
    public Codes Codes { get; set; } = default!;
}

public class Codes
{
    [JsonProperty("admin_district")]
    public string AdminDistrict { get; set; } = default!;

    [JsonProperty("admin_county")]
    public string AdminCounty { get; set; } = default!;
}