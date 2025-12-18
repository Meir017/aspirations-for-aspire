using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Meir.Aspire.Testing;

public class AspireIntegrationTestFixture<TEntryPoint>() 
    : DistributedApplicationFactory(typeof(TEntryPoint), []), 
      IAsyncLifetime 
    where TEntryPoint : class
{
    public DistributedApplication App { get; private set; } = null!;

    public ResourceNotificationService ResourceNotificationService => 
        App.Services.GetRequiredService<ResourceNotificationService>();

    protected override void OnBuilt(DistributedApplication application)
    {
        App = application;
        base.OnBuilt(application);
    }

    protected override void OnBuilderCreated(DistributedApplicationBuilder applicationBuilder)
    {
        // Configure HTTP client defaults for better resilience during tests
        applicationBuilder.Services.ConfigureHttpClientDefaults(http => 
            http.AddStandardResilienceHandler());

        base.OnBuilderCreated(applicationBuilder);
    }

    public Task InitializeAsync() => StartAsync().WaitAsync(TimeSpan.FromMinutes(10));

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}
