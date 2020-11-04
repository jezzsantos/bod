# Bod
[![Build status](https://ci.appveyor.com/api/projects/status/gb2pso0gypfuk8pf?svg=true)](https://ci.appveyor.com/project/JezzSantos/bod)

This project was inspired by the patterns in the Reference Implementation of [QueryAny](https://github.com/jezzsantos/queryany/samples/ri)

# Automated Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started)  for details on what you need installed on your machine.
1. Build the solution.
1. Run all tests in the solution.

> Note: if the integration tests for one of the repositories fail, its likely due to the fact that you don't have that technology installed on your local machine, or that you are not running your IDE as Administrator, and therefore cant start/stop those local services. Refer to step 1 above.

# Local Development, Debugging and Manual Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started)  for details on what you need installed on your machine.
1. You will need to start the Azure CosmosDB emulator on your machine  (from the Start Menu).
1. Ensure that you have manually created a new CosmosDB database called:  `Production`.
1. Start the `CarsApi` and `PersonsApi` projects locally with F5. (A  browser should open to API documentation site for both sites).

To manually test everything is working, or debug the code:

1. Navigate to: [GET https://localhost:5001/clinics/available](https://localhost:5001/clinics/available)  (you should get an empty array of cars in response)
1. Using a tool like PostMan or other REST client, create a new car by  calling: `POST /cars` with a JSON body like this: `{"Year":  2020,"Make": "Honda","Model": "Civic"}`


# Development Tasks

## Creating a New CRUD API

This is the  process for creating a new API from scratch in this codebase. It will require numerous new projects, new classes, new interfaces, tests, etc.  

We are going to build a new API for a new concept called: `Industry`. 

This new concept will use the following patterns:
1. The same repository for the read (`IQueryStorage<TDto>`) and write side (`ICommandStorage<TEntity>`). Making it a traditional CRUD based implementation.
1. It will use a domain specific repository pattern `IIndustryStorage` to  abstract away the actual persistence technology and persistence code.
1. It will use basic DDD patterns and principles including entities and value objects. It will not use an aggregate root (that is reserved for a event sourcing API - see later).
1. It will use the `PersisableEntityBase` class for its root aggregate, and all child entities. These entities support persistence required for CRUD based concepts.
1. It will not require any ReadModels.
1. Communication between domains is done strictly via HTTP service clients. 

All layers of this new concept will be separated from the API host down to the domain. That means:
 1. The API will be hosted in its own web host.
 1. All types specific to this concept (Application + Domain + Infrastructure layers) will be kept in separate assemblies.
 
 This means that at any time, this concept can be combined with or separated out into its own distributed service, without having to tease apart any coupling it has on other domains, applications or APIs. 
 
> In some cases you may want to  combine the API into an existing web host such as into the `MeetingsApi` project, this is fine, but you must be careful not to combine other layers and types into the same assemblies as other types. The separation is critical down the track when it comes time to split things up.  

### API Layer

The API layer is composed of one or more physical web hosts. API's can be hosted in their own  web host (distributed architectures - i.e. micro-services) or hosted in same web host as other API's (i.e. monolithic API). You can also decide to host all API's across one or more larger web hosts to optimize cost.

#### Shared API

In this scheme we add the API endpoints for the new API to an existing web host (such as to the `MeetingsApi` project). 

Open the `Infrastructure/Web/MeetingsApi` project, and add a new folder called `Industries` to the `Services` folder.

#### Standalone API

In this scheme we add the API endpoints for the new API to an new web host project called `IndustriesApi`. 

For this you will need a new web host project.

Create 3 new projects:
1. In the Infrastructure/Web folder, a project called: `IndustriesApi` (Class Library)
1. In the Infrastructure/Web/Tests folder, a project called: `IndustriesApi.UnitTests` (Unit Test Project)
1. In the Infrastructure/Web/Tests folder, a project called: `IndustriesApi.IntegrationTests` (Unit Test Project)

Edit the project file (select in Explorer, and then F4) for each project, and add the `<RootNamespace>` and `<IsPackable>false</IsPackable>` elements to the first `<PropertyGroup>`

Copy the following files from an existing API project to your new project `IndustriesApi`:
* `Startup.cs`
* `ServiceHost.cs`
* `Program.cs`
* `wwwroot` folder
* `appsettings.json`

Adjust the namespaces of all files.

#### API Service

Now regardless of your host model, you need to create a web service.

Create a new folder called `Services` and create a new class called `IndustriesService.cs`. Replace the generated class with the live template `service`

Sketch out the API signatures (Get, Post, Put and Delete methods) that you wish to have for your API. The examples that follows shows a GET and POST API.

Now, for each new request type (i.e. `CreateIndustryRequest`) you need to create that class in the `Application/Api.Interfaces` project, within a folder called `ServiceOperations/Industries`

for example:
```
    public class SearchIndustriesRequest : SearchOperation<SearchIndustriesResponse>
    {
        
    }

    public class SearchIndustriesResponse : SearchOperationResponse
    {
        public List<Industry> Industries { get; set; }
    }
```
and
```
    public class CreateIndustryRequest : PostOperation<CreateIndustryResponse>
    {
        public string Name { get; set; }
    }

    public class CreateIndustryResponse
    {
        public Industry Industry { get; set; }
    }
```

Make sure you add the various properties to each request class.  Use primitive property types only (i.e. `string`, `DateTime`, `int` etc)

Now refactor `SearchIndustriesResponse` and `CreateIndustryResponse` into their own files.

Now create a new DTO class called `Industry` in the project folder `Application/Application.Common/Resources`, and derive it from `IIdentifableResource` and `IQueryableEntity`

for example:
```
    [EntityName("Industry")]
    public class Industry : IIdentifiableResource, IQueryableEntity
    {
        public string Id { get; set; }
    }
```
Now, we need to create a request validator for each request type you defined above.

Create a new class called `CreateIndustryResponseValidator` in the `Infrastructure/Web/IndustriesApi/Services/Industries` folder. Replace the generated class with the live template `validator`

for example:
```
    internal class CreateIndustryRequestValidator : AbstractValidator<CreateIndustryRequest>
    {
        public CreateIndustryRequestValidator()
        {
            RuleFor(dto => dto.Name)
                .NotEmpty()
                .WithMessage(Resources.CreateIndustryRequestValidator_InvalidName);
            RuleFor(dto => dto.Name)
                .Matches(Validations.Industry.Name)
                .WithMessage(Resources.CreateIndustryRequestValidator_InvalidName);
        }
    }
```

Then add a new folder called `Properties` to the project, and add a new `Resources.resx` file to it.

In the resource file add validation messages for each property and boundary condition.

for example:

`CreateIndustryRequestValidator_InvalidName = The Name of the industry was invalid or missing`

Then add a `RuleFor()` statement for each property of the request type. (Using `FluentValidation` statements. Copy patterns in other validators)

Now create a new unit test class for the validator in the folder `Infrastructure/Web/Tests/IndustriesApi.UnitTests` project (or if sharing a web host, create this folder `Infrastructure/Web/Tests/MeetingsApi.UnitTests/Services/Industries`)
 
 Call the test file `CreateIndustryRequestValidatorSpec`. Replace the generated class with the live template `testcvalidator`

Now, add the unit tests to check for all boundary conditions of all request properties. See other validators for examples, and follow the same patterns.

### Application Layer

You never combine or share applications.

Create 2 new projects:
1. In the Application folder, a project called: `IndustriesApplication` (Class Library)
1. In the Application/Tests folder, a project called: `IndustriesApplication.UnitTests` (Unit Test Project)

Edit the project file (select in Explorer, and then F4) for each project, and add the `<RootNamespace>` and `<IsPackable>false</IsPackable>` elements to the first `<PropertyGroup>`

In the `IndustriesApplication` project, add anew lass called `IndustriesApplication`, and derive from `ApplicationBase` and `IIdustriesApplication`.

for example:

```
    public class IndustriesApplication : ApplicationBase, IIndustriesApplication
    {
    }

    public interface IIndustriesApplication
    {
    }
```
Refactor `IIndustriesApplication` into its own file

Now add the various methods you need to `IIndustriesApplication` from the `IndustriesService` class in `Infrastructure/Web/IndustriesApi/Services/Industries/Industries.cs`

for example:

```
    public interface IIndustriesApplication
    {
        SearchResults<Industry> SearchAllIndustries(ICurrentCaller caller, SearchOptions searchOptions, GetOptions getOptions);

        Industry CreateIndustry(ICurrentCaller caller, string name);
    }
```

Now, implement the new methods in `IndustriesApplication`, and add a constructor similar to this:

```
        private readonly ILogger logger;
        private readonly IIdentifierFactory idFactory;
        private readonly IIndustriesStorage storage;

        public IndustriesApplication(ILogger logger, IIdentifierFactory idFactory, IIndustriesStorage storage)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            storage.GuardAgainstNull(nameof(storage));
            this.logger = logger;
            this.idFactory = idFactory;
            this.storage = storage;
        }
```
Then flesh out the methods, using storage, domain entities and domain services, similar to this pattern:

```
        public SearchResults<Industry> SearchAllIndustries(ICurrentCaller caller, SearchOptions searchOptions, GetOptions getOptions)
        {
            caller.GuardAgainstNull(nameof(caller));

            var tags = this.storage.SearchAll(searchOptions);

            this.logger.LogInformation("All industries were retrieved by {Caller}", caller.Id);

            return searchOptions.ApplyWithMetadata(tags);
        }

        public Industry CreateIndustry(ICurrentCaller caller, string name)
        {
            caller.GuardAgainstNull(nameof(caller));
            name.GuardAgainstNullOrEmpty(nameof(name));

            var tag = new IndustryEntity(this.logger, this.idFactory, name);

            var created = this.storage.AddNew(tag);

            this.logger.LogInformation("The industry {Id} was created by {Caller}", created.Id, caller.Id);

            return created;
        }
```

Add a new folder called `Storage` and create a new interface called `IIndustriesStorage.cs`.

for example:

```
    public interface IIndustriesStorage
    {
        List<Industry> SearchAll(SearchOptions searchOptions);

        Industry AddNew(IndustryEntity tag);
    }
```

Now create a new unit test class for the application in the folder `Application/Tests/IndustriesApplication.UnitTests` project, called `IndustriesApplicationSpec`.

Now, add the unit tests to test out the functionality in the application.

### Domain Layer

You never combine or share domains.

Create 2 new projects:
1. In the Domain folder, a project called: `IndustriesDomain` (Class Library)
1. In the Domain/Tests folder, a project called: `IndustriesDomain.UnitTests` (Unit Test Project)

Edit the project file (select in Explorer, and then F4) for each project, and add the `<RootNamespace>` and `<IsPackable>false</IsPackable>` elements to the first `<PropertyGroup>`

In the `IndustriesDomain` project add the following as the last element within the `<Project>` node:

```
    <ItemGroup>
        <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
            <_Parameter1>$(AssemblyName).UnitTests</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
``` 

In the `IndustriesDomain` project, create a new folder called `Properties` and add a `Resources.resx` file.

Create a new class called `IndustryEntity` and derive it from `PersistableAggregateRootBase`.

Add the following constructors and methods:

```
    public class IndustryEntity : PersistableAggregateRootBase
    {
        public IndustryEntity(ILogger logger, IIdentifierFactory idFactory, string name) : base(logger, idFactory)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.Industry.Name, nameof(name), Resources.IndustryEntity_InvalidName);

            Name = name;
        }

        private IndustryEntity(ILogger logger, IIdentifierFactory idFactory, Identifier identifier, string name) : base(
            logger, idFactory, identifier)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));

            Name = name;
        }

        public string Name { get; private set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(Name), Name);
            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            Name = properties.GetValueOrDefault<string>(nameof(Name));
        }

        public static EntityFactory<IndustryEntity> Instantiate()
        {
            return (identifier, container, properties) => new IndustryEntity(container.Resolve<ILogger>(),
                container.Resolve<IIdentifierFactory>(), identifier,
                properties.GetValueOrDefault<string>(nameof(Name)));
        }
    }
```

If you add/change any property values in the constructors of your entity, you will need to update the `Dehydrate` and `Rehydrate` and `Instantiate` methods to reflect those changes.

In the `Domain/IndustryDomain.UnitTests` project add a new test files called `IndustryEntitySpec`, and add the following code:

```
    [TestClass, TestCategory("Unit")]
    public class IndustryEntitySpec
    {
        private IndustryEntity entity;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("atagid".ToIdentifier);
            this.entity = new IndustryEntity(this.logger.Object, this.identifierFactory.Object,
                "aname");
        }

        [TestMethod]
        public void WhenCreateIndustryAndInvalidName_ThenThrows()
        {
            FluentActions.Invoking(() => new IndustryEntity(this.logger.Object, this.identifierFactory.Object, "^aninvalidname"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.IndustryEntity_InvalidName);
        }

        [TestMethod]
        public void WhenCreateIndustryAndNotUniqueName_ThenThrows()
        {
            this.industriesService.Setup(ts => ts.EnsureIndustryIsUnique(It.IsAny<string>()))
                .Returns(false);

            FluentActions.Invoking(() =>
                    new IndustryEntity(this.logger.Object, this.identifierFactory.Object, "aname"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.IndustryEntity_NotUniqueName);
        }
    }

```

### Storage Layer

You never combine or share storage layers.

Create 1 new project:
1. In the Infrastructure folder, a project called: `IndustriesStorage` (Class Library)

Edit the project file (select in Explorer, and then F4) for each project, and add the `<RootNamespace>` and `<IsPackable>false</IsPackable>` elements to the first `<PropertyGroup>`

Create a new class called `IndustriesStorage`, and derive it from `IIndustriesStorage`

Implement two different constructors, one used from the web host and one from testing.

for example:

```
    public class IndustriesStorage : IIndustriesStorage
    {
        private readonly ICommandStorage<IndustryEntity> industryCommandStorage;
        private readonly IQueryStorage<Industry> industryQueryStorage;

        public IndustriesStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            this.industryQueryStorage = new GeneralQueryStorage<Industry>(logger, domainFactory, repository);
            this.industryCommandStorage = new GeneralCommandStorage<IndustryEntity>(logger, domainFactory, repository);
        }

        public IndustriesStorage(IQueryStorage<Industry> industryQueryStorage,
            ICommandStorage<IndustryEntity> industryCommandStorage)
        {
            industryQueryStorage.GuardAgainstNull(nameof(industryQueryStorage));
            industryCommandStorage.GuardAgainstNull(nameof(industryCommandStorage));
            this.industryQueryStorage = industryQueryStorage;
            this.industryCommandStorage = industryCommandStorage;
        }

        public List<Industry> SearchAll(SearchOptions searchOptions)
        {
            return this.industryQueryStorage.Query(Query.From<Industry>()
                    .WhereAll()
                    .WithSearchOptions(searchOptions))
                .Results;
        }

        public Industry AddNew(IndustryEntity industry)
        {
            return this.industryCommandStorage
                .Upsert(industry)
                .ToIndustry();
        }
    }

    internal static class IndustryConversionExtensions
    {
        public static Industry ToIndustry(this IndustryEntity entity)
        {
            var industry = entity.ConvertTo<Industry>();
            industry.Id = entity.Id;

            return industry;
        }
    }
```

### Integration Testing

At this point all your unit tests should all be green.

Open the `ServiceHost.cs` file in the web host project where your API is hosted, and add the following to the array of assemblies in the variable `AssembliesContainingDomainEntities` variable:

for example:

```
        public static readonly Assembly[] AssembliesContainingDomainEntities = new[]
        {
            typeof(EntityEvent).Assembly,
            typeof(MeetingEntity).Assembly,
            typeof(TagEntity).Assembly,
            typeof(IndustryEntity).Assembly
        };
```

Now, re-run all unit tests and make sure everything is still green.

Now we can focus on integration testing

In your `IndustriesApi.IntegrationTests` project (or if sharing a web host, create this folder `Infrastructure/Web/Tests/MeetingsApi.IntegrationTests`) add a new spec called `IndustriesApiSpec`

Implement integration tests testing your new API's. Use your API to create the data you need. You may need a few extra endpoints to do this. You can mark them as `TESTINGONLY`, so they dont get shipped to production.

for example:

```
[TestClass, TestCategory("Integration.Web")]
    public class IndustriesApiSpec
    {
        private const string ServiceUrl = "http://localhost:2004/";
        private static IWebHost webHost;
        private static IQueryStorage<Industry> industryQueryStorage;
        private static ICommandStorage<IndustryEntity> industryCommandStorage;
        private static IRepository inMemRepository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            ServiceStackHost.Instance?.Dispose();
            webHost = WebHost.CreateDefaultBuilder(null)
                .UseModularStartup<Startup>().UseUrls(ServiceUrl)
                .UseKestrel()
                .ConfigureLogging((ctx, builder) => builder.AddConsole())
                .Build();
            webHost.Start();

            // Override services for testing
            var container = HostContext.Container;
            inMemRepository = new InProcessInMemRepository();

            industryQueryStorage = new GeneralQueryStorage<Industry>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);
            industryCommandStorage = new GeneralCommandStorage<IndustryEntity>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);

            container.AddSingleton(industryQueryStorage);
            container.AddSingleton(industryCommandStorage);
            container.AddSingleton<IIndustriesStorage>(c =>
                new IndustriesStorage.IndustriesStorage(industryQueryStorage, industryCommandStorage));

            //HACK: subscribe again (see: https://forums.servicestack.net/t/integration-testing-and-overriding-registered-services/8875/5)
            HostContext.AppHost.OnAfterInit();
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
            webHost?.Dispose();
        }

        [TestInitialize]
        public void Initialize()
        {
            industryQueryStorage.DestroyAll();
            industryCommandStorage.DestroyAll();
        }

        [TestMethod]
        public void WhenCreateIndustry_ThenReturnsIndustry()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var industry = client.Post(new CreateIndustryRequest
            {
                Name = "aname"
            }).Industry;

            industry.Name.Should().Be("aname");
        }

        [TestMethod]
        public void WhenGetAllAndNoIndustries_ThenReturnsNone()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var industries = client.Get(new SearchAllIndustriesRequest()).Industries;

            industries.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenGetAllAndSomeIndustries_ThenReturnsIndustries()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var industry1 = client.Post(new CreateIndustryRequest {Name = "aindustryname1"}).Industry;
            var industry2 = client.Post(new CreateIndustryRequest {Name = "aindustryname2"}).Industry;

            var industries = client.Get(new SearchAllIndustriesRequest()).Industries;

            industries.Count.Should().Be(2);
            industries[0].Name.Should().Be(industry1.Name);
            industries[1].Name.Should().Be(industry2.Name);
        }
```

These tests should fail at this moment, since we are missing a bunch of services in our DI container in the web host.

In the `ServiceHost.cs` file found in your web host project, add the following declarations:

```
            container.AddSingleton<IIndustriesStorage>(c =>
                new IndustriesStorage.IndustriesStorage(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(),
                    ResolveRepository(c)));
            container.AddSingleton<IIndustriesApplication, IndustriesApplication.IndustriesApplication>();

```

Now, your new integration tests should run green.

Congrats, you created your API, and tested it!


## Creating a New Event Sourcing API

This is the alternative process for creating a new API from scratch in this codebase. Like the previous section, it will require numerous new projects, new classes, new interfaces, tests, etc.
  
This new concept will use the following patterns:
1. It will have a specific ReadModel with Projections for the read side of all GET based APIs.
1. It will use an event sourcing strategy for the write side (`IEventStreamStorage<TEntity>`).
1. It will use a domain specific repository pattern `IIndustryStorage` to that abstract away the actual persistence technology and persistence code.
1. It will use comprehensive DDD patterns and principles including aggregate roots, entities and value objects.
1. It will use the `EntityBase` class for any child entities and use `AggregateRootBase` for each  root aggregate. Entities do not support persistence, and support eventing instead. Aggregate roots also support eventing and persistence to an event store.
1. Communication between domains is done either via HTTP service clients, or an event bus. 

### API Layer

Same as for the CRUD API

### Application Layer

Similar to the CRUD API, except for the fact that:
 1. The root aggregate is the only entity that can be seen by the application layer. All access to all other entities is via the root.
 1. There should be only one aggregate root per application.
 1. Applications cannot talk directly to each other, and must communicate via service clients, or event bus.

### Storage Layer

Similar to the CRUD API, except for the fact that:
 1. `ICommandStorage<TEntity>` is replaced by `IEventStreamStorage<TEntity>`.

### Domain Layer

Similar to the CRUD API, except for the fact that:
1. The root entity becomes the aggregate root, and derives from `AggregateRootBase`, and must raise events for any change in state of any child valueobject or child entity.
1. All other entities derive from `EntityBase`, and must raise events. (There is no support for persistence)