## DotNetCore Configuration Templates 

**DotNetCore.Configuration.Formatter** is a simple Configuration ASP.NET Core Templates for
[**Microsoft.Extensions.Configuration**](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0).
It is used [**Configuration Providers**](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#configuration-providers)
for generating configuration value text output by substituting Configuration **Key** with its **Value**.

Annotations '**\{...\}**' in the template refer to elements of the configuration data structure.
It allows to application configuration values to be resloved and formatted with using key values of other configuration sections and providers.

### Nuget.org

- Nuget package [DotNetCore.Configuration.Formatter](https://www.nuget.org/packages/DotNetCore.Configuration.Formatter/)

# Version: 7.2.x
- Add **\{Key??\{DefaultKey}}** full support
- Big configuration perfomance improvement

# Version: 7.1.x
**.Net Core App support**
- Supports: **net 7.0**,  **net48**, **netstandard2.0**, **netstandard2.1**
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
  


## Azure Integration 

- Can be used in conjunction with [DotNetCore Azure Configuration KeyVault Secrets](https://github.com/Wallsmedia/DotNetCore.Azure.Configuration.KvSecrets).
- Can be used in conjunction with [DotNetCore Azure Configuration KeyVault Certificates](https://github.com/Wallsmedia/DotNetCore.Azure.Configuration.KvCertificates)

## How to use

Example of using 

### Define the ApplicationConfiguration.cs:

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


### Define the following appsettings.json:

``` JSON 
{
  ApplicationConfiguration:{
     "IsDocker": "{DOTNET_RUNNING_IN_CONTAINER??false}",
     "RunLocation":"{host_env??local}"
     "BusConnection":"{secret:{host_env}:BusConnection}"
     "DbConnection":"{secret:{host_env}:DbConnection}",
     "CosmosDbConnection":"{secret:{host_env}:CosmosDbConnection}"
  }
}
```



### Environment Variables to :

```
DOTNET_RUNNING_IN_CONTAINER=true
...
host_env=dev
```

### Define in the Secret file or User secrets or [DotNetCore.Azure.Configuration.KvSecrets](https://www.nuget.org/packages/DotNetCore.Azure.Configuration.KvSecrets)...

```
{
    "secret:dev:BusConnection":"... azure bus endpoint ... ... "
    "secret:dev:DbConnection":"... sql connection string ... ... "
    "secret:dev:CosmosDbConnection":"... mongo db connection string ... ... "

    "secret:uat:BusConnection":"... azure bus endpoint ... ... "
    "secret:uat:DbConnection":"... sql connection string ... ... "
    "secret:uat:CosmosDbConnection":"... mongo db connection string ... ... "
}
```

### In the Startup.cs


``` CSharp

     var applicationConfig = Configuration.ApplyConfigurationFormatter()
     .GetSection(nameof(ApplicationConfiguration))
     .Get<ApplicationConfiguration>();
  ```

or with **shorthand** 

``` CSharp

     var applicationConfig = Configuration.GetTypeNameFormatted<ApplicationConfiguration>();

```

The Web Service will be provided with filly resolved configuration with Azure Key Vault secrets. 

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

