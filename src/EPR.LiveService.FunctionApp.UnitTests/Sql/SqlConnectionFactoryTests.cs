using EPR.LiveService.FunctionApp.Sql;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.LiveService.FunctionApp.UnitTests.Sql;

[TestClass]
public class SqlConnectionFactoryTests
{
    [TestMethod]
    public async Task CreateConnectionAsync_WithUnknownTarget_ShouldThrowArgumentExceptionListingRegisteredTargets()
    {
        var options = Options.Create(new Dictionary<string, SqlTargetOptions>
        {
            ["accounts"] = new SqlTargetOptions { ConnectionString = "Server=fake;" },
            ["synapse"] = new SqlTargetOptions { ConnectionString = "Server=fake;" }
        });
        var factory = new SqlConnectionFactory(options);

        var act = async () => await factory.CreateConnectionAsync("does-not-exist");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*does-not-exist*")
            .WithMessage("*accounts*")
            .WithMessage("*synapse*");
    }
}
