## DotNetCore Configuration formatter

### Development Supported by JetBrains Open Source Program:

**Rider** <a href="https://www.jetbrains.com/?from=XmlResult"> <img src="https://github.com/Wallsmedia/XmlResult/blob/master/Logo/rider/logo.png?raw=true" Width="40p" /></a> **Fast & powerful,
cross platform .NET IDE**

**ReSharper** <a href="https://www.jetbrains.com/?from=XmlResult"> <img src="https://github.com/Wallsmedia/XmlResult/blob/master/Logo/resharper/logo.png?raw=true" Width="40p" /></a> **The Visual Studio Extension for .NET Developers**

#### DotNetCore.Configuration.Formatter

#### Version 5.0.0
  - Support SDK v.5.0-*

**DotNetCore.Configuration.Formatter** is a simple string formatter confined with **Microsoft.Extensions.Configuration**. It allows to application configuration values 
with using key values of other sections. For example using values from "secret" configuration provider just referencing for example **"{secret:db-{environment}-password}"**.    

### Nuget.org

- Nuget package [DotNetCore.Configuration.Formatter](https://www.nuget.org/packages/DotNetCore.Configuration.Formatter/)


### Format syntax

 - " some text **\{Key}** other text " -  If the **Key** will be located in a map (Dictionary) it will be replaced with a **value**.
 
 - " some text **\{NotFound}** other text " - If the **NotFound** will not be located in a map it will be replaced with the  **?NotFound?**.
 
 - " some text **\{NotFound}??Default** other text " - If the **NotFound** will not be located in a map it will be replaced with the **Default**.
   
 - " some text **\{Complex{Complex{ComplexKey3}Key2}Key1}** other text " -  Supports recursive keys substitution, it will be replaced with a final constructed key **value**.

#### Example for a string formatting:

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

### Asp Net Core  

The main area of using is to format (i.e. substitute ) configuration values with other value defined in other configuration providers 
like Azure KeyVault Secrets, AppRuntime.

