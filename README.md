# Inflatable

[![Build status](https://ci.appveyor.com/api/projects/status/nr3ltljg4rkfwnph?svg=true)](https://ci.appveyor.com/project/JaCraig/inflatable)

Inflatable is an ORM. It supports .Net Core as well as full .Net.

## Setting Up the Library

Inflatable  relies on [Canister](https://github.com/JaCraig/Canister) in order to hook itself up. In order for this to work, you must do the following at startup:

    var MyServices = new ServiceCollection();
    MyServices.AddCanisterModules();
    ...
					
The AddCanisterModules function is an extension method that registers it with the IoC container. When this is done, Inflatable is ready to use.

## Basic Usage

The main class of interest is the DbContext class:

	var Results = DbContext<MyPoco>.CreateQuery().Where(x => x.MyProperty == 12).ToList();
	
The DbContext class has the ability to do IQueryables, however only basic items are supported including Where, Select, OrderBy, Distinct, First, Single, Take, and the variations of those items. Functions like GroupBy, Union, and Include are not implemented at the moment. For more complex queries, you can drop down to actual SQL:

    var Results = await DbContext<MyPoco>.ExecuteAsync("SELECT * FROM MyTable",CommandType.Text,"MyConnectionString");
	
In order to save or delete an object, you must create an instance of DbContext or a Session object:

    await new DbContext<MyPoco>().Save(MyObject).ExecuteAsync();
	
Or you can resolve the DbContext from the service provider for your app.
	
