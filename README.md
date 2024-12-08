## DotNetCore Generic Configuration

**DotNetCore.Configuration.Formatter** creates a new configuration values by substituting IConfiguration Keys with Values from other IConfiguration Keys.

It is used in addtion to [**Microsoft.Extensions.Configuration**](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0)
for providing generic configuration values based on simple "annotation format".

Annotations '**\{...\}**' in the **value** refer to other key elements in the IConfiguration data structure.
It allows to application configuration values to be resloved (renderd) with using key values of other configuration sections and configuration providers.

### Nuget.org

- Nuget package [DotNetCore.Configuration.Formatter](https://www.nuget.org/packages/DotNetCore.Configuration.Formatter/)


# Version: 9.0.x
**.Net Core App support**
- Supports: **net 9.0**, **netstandard2.0**, **netstandard2.1**
- Azure Function Support via **ResolveAllKeyValues**
- **ResolveKeyValue** - Format/resolve a key reference value and replace it in the configuration. 
- **ResolveAllKeyValues** - Format/resolve all key reference values and replace them in the configuration. 
- **AllConfigurationKeys** - Get all keys from the configuration.
- **\{Key??}** - empty string feature.
- **\{Key??null}** - null feature. 

## Annotation format syntax

|  Annotation   | Definition  |
-----------------------------------------------   | ---  |
 **\{ Key }**  |  If the **Key**  reference will be resolved; it will be replaced with a **value**. If **\{Key}** is not found, it will not be replaced and recursive references substitution will be cancelled,i.e JSON frendly.
 **\{{{Key3}Key2}Key1}**   |  Supports complex and multiply recursive references substitution, it will be replaced with a final constructed reference **value**.
 **\{Key??DefaultValue}**   | If the **Key** reference will not be resolved in it will be replaced with the **Default**.
 **\{Key??\{DefaultKey}}** | If the **Key** reference will not be resolved in it will be replaced with the **DefaultKey** value.
  **\{Key??}**   | If the **Key** reference will not be resolved in it will be replaced with the **string.Empty**.
 **\{Key??null}**   | If the **Key** reference will not be resolved in it will be replaced with **null**.
  


## How to use 

### Lets define the ApplicationConfiguration class:

``` CSharp

public class ApplicationConfiguration 
{
     public bool IsDocker {get; set;}
     public string RunLocation {get; set;}
     public string BusConnection {get; set;}
     public string DbConnection {get; set;}
     public string CosmosDbConnection {get; set;}
}
```

### Lets define the typical use cases:
 - We want that all values will have deffernt settings for different environments;
 - Environment will be defined by external environment variable 
     - host_env = [dev|uat...] with default value **loc**

### Define the generic appsettings.json:

``` JSON 
{
  ApplicationConfiguration:{
     "IsDocker": "{DOTNET_RUNNING_IN_CONTAINER??false}",
     "RunLocation":"{host_env??loc}"
     "BusConnection":"{secret:{host_env??loc}:BusConnection}"
     "DbConnection":"{secret:{host_env??loc}:DbConnection}",
     "CosmosDbConnection":"{secret:{host_env??loc}:CosmosDbConnection}"
  }
}
```

### Define the secrets.json file, or use [DotNetCore.Azure.Configuration.KvSecrets](https://www.nuget.org/packages/DotNetCore.Azure.Configuration.KvSecrets), or write own secret configuration provider.

Where you define you secrets
``` JSON
{
    "secret:loc:BusConnection":"... azure bus endpoint ... ... "
    "secret:loc:DbConnection":"... sql connection string ... ... "
    "secret:loc:CosmosDbConnection":"... mongo db connection string ... ... "

    "secret:dev:BusConnection":"... azure bus endpoint ... ... "
    "secret:dev:DbConnection":"... sql connection string ... ... "
    "secret:dev:CosmosDbConnection":"... mongo db connection string ... ... "

    "secret:uat:BusConnection":"... azure bus endpoint ... ... "
    "secret:uat:DbConnection":"... sql connection string ... ... "
    "secret:uat:CosmosDbConnection":"... mongo db connection string ... ... "
}
```

### In the Program.cs

Add you secrets into Configuration  

``` CSharp
  // Adds secrets.json.
  builder.Configuration.AddJsonFile("secrets.json");

  // Or;
  // Adds Azure Key Valt configuration provider.
  builder.Configuration.AddAzureKeyVault(....);

  // Or;
  // Adds your own configuration provider.
  builder.Configuration.AddMySecretsProvider(....);
```

Get generic app configuration rendered by "host_env" 

``` CSharp
     var applicationConfig = builder.Configuration.ApplyConfigurationFormatter()
     .GetSection(nameof(ApplicationConfiguration))
     .Get<ApplicationConfiguration>();
     builder.Services.AddSingleton(applicationConfig);
  ```

or with **shorthand** 

``` CSharp

     var applicationConfig = builder.Configuration.GetTypeNameFormatted<ApplicationConfiguration>();
     builder.Services.AddSingleton(applicationConfig);
```


## Azure Function Support

Some software used a dynamic IConfiguration in the code. In this case  "DotNetCore Configuration Templates" doesn't work.
For example it is Azure Functions. There added a special feature **ResolveKeyValue**.

``` CSharp
    var configuration = ... // Get IConfiguration
    var keyToResolve = "MyConfigurationKey".
    var isUpdated = configuration.ResolveKeyValue(keyToResolve);
```

Resolve all keys in a configuration.

``` CSharp
    var configuration = ... // Get IConfiguration
    var isUpdated = configuration.ResolveAllKeyValues();
```

## Tested with Azure Configuration Providers

- Can be used in conjunction with [DotNetCore Azure Configuration KeyVault Secrets](https://github.com/Wallsmedia/DotNetCore.Azure.Configuration.KvSecrets).
- Can be used in conjunction with [DotNetCore Azure Configuration KeyVault Certificates](https://github.com/Wallsmedia/DotNetCore.Azure.Configuration.KvCertificates)
