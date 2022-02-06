//   \\      /\  /\\
//  o \\ \  //\\// \\
//  |  \//\//       \\
// Copyright (c) i-Wallsmedia 2022. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Xunit;

namespace DotNetCore.Configuration.Formatter.Test
{
    public class ConfigurationAppExample
    {
        public string ConnectionUrl { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class ConfigurationWrapperUnitTest
    {
        IConfiguration configuration;
        string json =
@"
{
    ""ConfigurationAppExample"": {
    ""ConnectionUrl"": ""{protocol}://sql-{app-env}-data-"",
    ""Name"": ""sql-{app-env}-example-login"",
    ""Password"": ""{secret:sql-service}""
  }

}
";
        public ConfigurationWrapperUnitTest()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonStream(StreamHelpers.StringToStream(json))
                .Build();
        }

        [Fact]
        public void AllConfigurationKeysTest()
        {

            // Arrange
            configuration["P: a: s: s: w: o: r: d"] = "{ secret: sql - service}";

            // Act
            var keys = ((IConfigurationRoot)configuration).AllConfigurationKeys();


            // Assert
            Assert.Equal(4, keys.Count);

        }

        [Fact]
        public void ResolveKeyValueConfigurationTest()
        {

            // Arrange
            configuration["secret:sql-service"] = "PassWord@01";

            // Act
            var changed = configuration.ResolveKeyValue("ConfigurationAppExample:Password");


            // Assert
            Assert.True(changed);
            Assert.Equal(configuration["secret:sql-service"], configuration["ConfigurationAppExample:Password"]);
        }

        [Fact]
        public void ResolveAllKeyValuesTest()
        {
            // Arrange
            // simulate add into configuration key values
            configuration["protocol"] = "protocol";
            configuration["app-env"] = "dev";
            configuration["secret:sql-service"] = "PASSWORD";
            // get wrapper
            var changed = configuration.ResolveAllKeyValues();

            // Act
            var configExample = configuration
                .GetSection(nameof(ConfigurationAppExample))
                .Get<ConfigurationAppExample>();

            // Assert
            Assert.True(changed);
            Assert.Equal(configuration["secret:sql-service"], configExample.Password);
            Assert.Equal("protocol://sql-dev-data-", configExample.ConnectionUrl);
            Assert.Equal("sql-dev-example-login", configExample.Name);
        }

        [Fact]
        public void GetClassConfigurationWrapperRootOnly()
        {
            // Arrange
            // simulate add into configuration key values
            configuration["protocol"] = "protocol";
            configuration["app-env"] = "dev";
            configuration["secret:sql-service"] = "PASSWORD";
            // get wrapper
            var configurationWrapper = configuration.UseFormater();

            // Act
            var configExample = configurationWrapper
                .GetSection(nameof(ConfigurationAppExample))
                .Get<ConfigurationAppExample>();

            // Assert
            Assert.Equal(configuration["secret:sql-service"], configExample.Password);
            Assert.Equal("protocol://sql-dev-data-", configExample.ConnectionUrl);
            Assert.Equal("sql-dev-example-login", configExample.Name);
        }

    }
}
