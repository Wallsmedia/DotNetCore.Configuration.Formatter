## DotNetCore Configuration Templates 

**DotNetCore.Configuration.Formatter** is a simple Configuration ASP.NET Core Templates for
[**Microsoft.Extensions.Configuration**](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0).
It is used various [**Configuration Providers**](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#configuration-providers)
for generating configuration value text output by substituting Configuration **Key** with its **Value**.

Annotations '**\{...\}**' in the template refer to elements of the configuration data structure.
It allows to application configuration values to be annotated and formatted with using key values of other configuration sections and providers.

### Nuget.org

- Nuget package [DotNetCore.Configuration.Formatter](https://www.nuget.org/packages/DotNetCore.Configuration.Formatter/)


## Annotation format syntax

|  Annotation   | Definition  |
-----------------------------------------------   | ---  |
  **\{Key}**  |  If the **Key**  reference will be located/resolved it will be replaced with a **value**.
  If **\{Key}** is not found |The reference will not be located it will be replaced with the  '**?NotFound?**`.
  **\{Key??Default}**   | If the **Key** reference will not be located in it will be replaced with the **Default**.
  **\{{{Key3}Key2}Key1}**   |  Supports recursive references substitution, it will be replaced with a final constructed reference **value**.


## How to use

For example you have the following:

##### Environment Variables set to :

```
DOTNET_RUNNING_IN_CONTAINER=true
...
host_environmet=datacenter
```

##### Secret KeyValaut Configuration Provider loads the following secrets:
[DotNetCore.Azure.Configuration.KvSecrets](https://www.nuget.org/packages/DotNetCore.Azure.Configuration.KvSecrets)


```
"secret:BusConnection":"... azure bus endpoint ... ... "
"secret:DbConnection":"... sql connection string ... ... "
"secret:CosmosDbConnection":"... mongo db connection string ... ... "
```

##### Web Service has the following appsettings.json:

``` JSON 
{
  ApplicationConfiguration:{
     "IsDocker": "{DOTNET_RUNNING_IN_CONTAINER??false}",
     "RunLocation":"{host_environmet??local}"
     "BusConnection":"{secret:BusConnection}"
     "DbConnection":"{secret:DbConnection}",
     "CosmosDbConnection":"{secret:CosmosDbConnection}"
  }
}
```

##### Web Service has the ApplicationConfiguration.cs

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

##### Web Service has the Startup.cs


``` CSharp

     var applicationConfig = Configuration.UseFormater()
     .GetSection(nameof(ApplicationConfiguration))
     .Get<ApplicationConfiguration>();

```

or with **shorthand** 

``` CSharp

     var applicationConfig = Configuration.GetTypeNameFormatted<ApplicationConfiguration>();

```

The Web Service will be provided with filly resolved configuration with Azure Key Vault secrets. 


## Dot Net Core

##### Example for a string formatting:

``` C#
 var keyValues = new Dictionary<string, string>()
 {
     ["Key-1"] = "Value-1",
     ["Key-Value-1"] = "Complex-Value-1"
 };
 var format = "Get the {Key-1} and complex: {Key-{Key-1}} and {none??Default} and {NotFound}";
 var formatted = format.FormatString(keyValues);

Formated string will be 
    "Get the Value-1 and complex: Complex-Value-1 and Default ?NotFound?"
```

