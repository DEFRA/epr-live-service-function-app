using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Http;

internal sealed class TestFunctionContext : FunctionContext
{
    public override string InvocationId { get; } = Guid.NewGuid().ToString();
    public override string FunctionId { get; } = "test-function";
    public override TraceContext TraceContext => throw new NotSupportedException();
    public override BindingContext BindingContext => throw new NotSupportedException();
    public override RetryContext RetryContext => throw new NotSupportedException();
    public override IServiceProvider InstanceServices { get; set; } = BuildServices();
    public override FunctionDefinition FunctionDefinition => throw new NotSupportedException();
    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
    public override IInvocationFeatures Features => throw new NotSupportedException();

    /// <summary>
    /// ReadFromJsonAsync/WriteAsJsonAsync resolve their serializer from
    /// IOptions<WorkerOptions> via InstanceServices. The real host wires
    /// this up inside Program.cs's ConfigureFunctionsWebApplication() call,
    /// which a unit test never runs - without registering it by hand here,
    /// every JSON-body test fails with "A serializer is not configured for
    /// the worker." JsonSerializerDefaults.Web matches the SDK's own default
    /// (camelCase, case-insensitive property matching), which is what
    /// production actually uses since Program.cs doesn't override it.
    /// </summary>
    private static IServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.Configure<WorkerOptions>(options =>
            options.Serializer = new JsonObjectSerializer(
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        return services.BuildServiceProvider();
    }
}
