using System.Text;
using EPR.LiveService.FunctionApp.Middleware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Middleware;

[TestClass]
public class ClientPrincipalDecoderTests
{
    private readonly ClientPrincipalDecoder _decoder = new();

    [TestMethod]
    public void TryDecodeClientPrincipal_WithValidHeader_ReturnsClaims()
    {
        const string json =
            """
            {
              "auth_typ": "aad",
              "claims": [
                { "typ": "name", "val": "Example User" },
                { "typ": "roles", "val": "Reader" }
              ]
            }
            """;
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var result = _decoder.TryDecode(header, out var claims);

        result.Should().BeTrue();
        claims.Should().BeEquivalentTo(
        [
            new ClientPrincipalClaim("name", "Example User"),
            new ClientPrincipalClaim("roles", "Reader")
        ]);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not-base64")]
    public void TryDecodeClientPrincipal_WithInvalidHeader_ReturnsFalse(string? header)
    {
        var result = _decoder.TryDecode(header, out var claims);

        result.Should().BeFalse();
        claims.Should().BeEmpty();
    }

    [TestMethod]
    public void TryDecodeClientPrincipal_WithInvalidJson_ReturnsFalse()
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{"));

        var result = _decoder.TryDecode(header, out var claims);

        result.Should().BeFalse();
        claims.Should().BeEmpty();
    }
}
