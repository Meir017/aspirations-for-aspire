# Aspire Resource Client Ready Libraries - AI Agent Instructions

## Project Overview
Collection of .NET Aspire extension libraries that provide event-driven resource initialization patterns. Three NuGet packages extend Aspire's resource model with `OnClientReady` callbacks for Azure CosmosDB, MongoDB, and Azure Storage.

**Tech Stack**: .NET 10.0, Aspire 13.1.0, C# with nullable reference types enabled

## Architecture Pattern

### Core Design: Event-Driven Resource Initialization
All libraries follow identical architectural patterns:

1. **Extension Methods** on Aspire resource builders (`IResourceBuilder<TResource>`)
   - Named `OnClientReady` for consistency across all resource types
   - Accept `Func<TEvent, CancellationToken, Task>` handlers
   - Return the builder for fluent chaining

2. **Marker Annotations** prevent duplicate event subscriptions
   - Private nested classes implementing `IResourceAnnotation`
   - Check existence with `TryGetLastAnnotation` before subscribing
   - Examples: `CosmosDBClientReadyAnnotation`, `MongoClientReadyAnnotation`

3. **Event Records** carry initialized client instances
   - Implement `IDistributedApplicationResourceEvent`
   - Named pattern: `{ResourceType}ReadyEvent` (e.g., `CosmosDBClientReadyEvent`)
   - Store both the resource and the initialized client/database/container

4. **Lifecycle Hooks**
   - Use `builder.OnResourceReady` to hook into Aspire's resource lifecycle
   - Initialize clients from `ConnectionStringExpression.GetValueAsync()`
   - Publish events via `builder.ApplicationBuilder.Eventing.PublishAsync()`

### Hierarchical Resource Pattern
CosmosDB demonstrates 3-level hierarchy (Client → Database → Container), MongoDB has 2 levels (Client → Database), Storage has 2 levels (BlobServiceClient → Container). Each level:
- Gets connection string from parent resource
- Creates appropriate client instance  
- Publishes typed ready event
- Subscribes handlers via private `OnEvent` helper

## Project Structure

```
src/{LibraryName}/              # NuGet package source
  {ProviderName}Extensions.cs   # Main extension methods (e.g., AzureCosmosExtensions.cs)
  Events/                       # Event record types
    {EventName}Event.cs
  *.csproj                      # Package metadata with IsPackable=true

test/{LibraryName}.Tests/       # xUnit integration tests
  {Provider}IntegrationTestFixture.cs  # Inherits AspireIntegrationTestFixture<TEntryPoint>
  {Provider}ResourceClientReadyTests.cs

playground/{Provider}.AppHost/  # Live demonstration apps
  Program.cs                    # Shows OnClientReady usage patterns
```

## Critical Conventions

### 1. Central Package Management
**All dependencies** managed via [Directory.Packages.props](../Directory.Packages.props):
```xml
<PackageVersion Include="Aspire.Hosting" Version="13.1.0" />
```
Projects reference without version: `<PackageReference Include="Aspire.Hosting" />`

### 2. Test Fixture Pattern
Use `AspireIntegrationTestFixture<TEntryPoint>` base class from [test/AspireIntegrationTestFixture.cs](../test/AspireIntegrationTestFixture.cs):
- Inherits `DistributedApplicationFactory` and `IAsyncLifetime`
- Provides `ResourceNotificationService` for health checks
- Configures HTTP resilience handlers automatically
- 10-minute startup timeout for resource initialization

```csharp
public class MyIntegrationTestFixture() : AspireIntegrationTestFixture<Program>
{
    public bool EventFired { get; private set; }
    
    protected override void OnBuilderCreated(DistributedApplicationBuilder builder)
    {
        builder.AddResource().OnClientReady(async (evt, ct) => EventFired = true);
        base.OnBuilderCreated(builder);
    }
}
```

### 3. Event Subscription Safety
Always check for existing annotations to prevent duplicate subscriptions:
```csharp
if (!builder.Resource.TryGetLastAnnotation<MyAnnotation>(out var existingEvent))
{
    builder.WithAnnotation(new MyAnnotation());
    // Subscribe only if annotation doesn't exist
}
```

### 4. Playground AppHost Structure
Demonstrate all resource levels in [playground/](../playground/) apps:
- Use `.RunAsEmulator()` for cloud resources in demos
- Show cascading `OnClientReady` calls at each level
- Console.WriteLine to demonstrate event firing

## Development Workflows

### Build & Test
```powershell
# Build entire solution (uses .slnx format)
dotnet build

# Run all integration tests (requires Docker for emulators)
dotnet test

# Run specific library tests
dotnet test test/Meir.Aspire.Azure.CosmosDB.ResourceClientReady.Tests
```

### Run Playground Apps
```powershell
cd playground/CosmosDB.AppHost
dotnet run
# Launches Aspire dashboard, watch console for OnClientReady events
```

### Package Creation
Projects have `IsPackable=true` and include:
- XML documentation (`GenerateDocumentationFile=true`)
- Package metadata (Authors, Description, Tags, License)
- RepositoryUrl and PackageProjectUrl

## Adding New Resource Types

1. **Create library project** in `src/Meir.Aspire.{Provider}.ResourceClientReady/`
   - Reference Aspire.Hosting and provider-specific packages
   - Enable nullable reference types and implicit usings

2. **Implement extension methods** following the pattern in [src/Meir.Aspire.Azure.CosmosDB.ResourceClientReady/AzureCosmosExtensions.cs](../src/Meir.Aspire.Azure.CosmosDB.ResourceClientReady/AzureCosmosExtensions.cs)
   - One `OnClientReady` per resource level
   - Private marker annotation classes
   - Typed event records in Events/ folder

3. **Create test fixture** inheriting `AspireIntegrationTestFixture<TEntryPoint>`
   - Hook OnBuilderCreated to add resources and subscribe to events
   - Store boolean flags for event verification
   - Assert with `ResourceNotificationService.WaitForResourceHealthyAsync()`

4. **Add playground demo** showing real-world usage at all resource levels

## Key Files to Reference
- [test/AspireIntegrationTestFixture.cs](../test/AspireIntegrationTestFixture.cs) - Base test fixture pattern
- [src/Meir.Aspire.Azure.CosmosDB.ResourceClientReady/AzureCosmosExtensions.cs](../src/Meir.Aspire.Azure.CosmosDB.ResourceClientReady/AzureCosmosExtensions.cs) - Complete 3-level hierarchy example
- [src/Meir.Aspire.MongoDB.ResourceClientReady/MongoDBHostingExtensions.cs](../src/Meir.Aspire.MongoDB.ResourceClientReady/MongoDBHostingExtensions.cs) - 2-level hierarchy pattern
- [Directory.Packages.props](../Directory.Packages.props) - All package versions
