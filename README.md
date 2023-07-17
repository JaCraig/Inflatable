# Inflatable

[![Build status](https://ci.appveyor.com/api/projects/status/nr3ltljg4rkfwnph?svg=true)](https://ci.appveyor.com/project/JaCraig/inflatable)

Inflatable is a feature-rich ORM (Object-Relational Mapping) library for .NET applications. It provides seamless integration with your data access layer, allowing you to interact with your database using a convenient and expressive API.

## Getting Started

To use Inflatable, you need to wire it up with you ServiceCollection. Follow the steps below to configure your application:

1. Install the Inflatable package from NuGet.

2. In your application's startup code, add the following lines to configure Canister:

    ```csharp
    var services = new ServiceCollection();
    services.AddCanisterModules();
    // ...
    ```

    The `AddCanisterModules()` extension method registers Inflatable with the IoC container.

3. With these steps completed, Inflatable is ready to be used within your application.

## Basic Usage

The primary class of interest in Inflatable is the `DbContext` class, which provides a rich set of features for querying and interacting with your database.

### Querying Data

To retrieve data from the database, use the `DbContext<T>` class with the `CreateQuery()` method:

```csharp
var results = DbContext<MyPoco>.CreateQuery().Where(x => x.MyProperty == 12).ToList();
```

The `CreateQuery()` method returns an `IQueryable<T>`, allowing you to chain additional query operations such as `Where`, `Select`, `OrderBy`, `Distinct`, `First`, `Single`, `Take`, and their variations. Please note that functions like `GroupBy`, `Union`, and `Include` are not currently implemented.

For more complex queries or when you need to execute raw SQL, you can use the `ExecuteAsync()` method:

```csharp
var results = await DbContext<MyPoco>.ExecuteAsync("SELECT * FROM MyTable", CommandType.Text, "MyConnectionString");
```

### Saving and Deleting Objects

To save or delete an object, you need to create an instance of `DbContext` or a `Session` object:

```csharp
await new DbContext<MyPoco>().Save(myObject).ExecuteAsync();
```

Alternatively, you can resolve the `DbContext` from the service provider in your application.

## Documentation

For detailed information on using Inflatable and its advanced features, refer to the [documentation](https://jacraig.github.io/Inflatable/) available on the project's website.

## Contributing

Contributions are welcome! If you have any bug reports, feature requests, or would like to contribute to the project, please check out the [contribution guidelines](https://github.com/JaCraig/Inflatable/blob/master/CONTRIBUTING.md).