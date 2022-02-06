//   \\      /\  /\\
//  o \\ \  //\\// \\
//  |  \//\//       \\
// Copyright (c) i-Wallsmedia 2022. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace DotNetCore.Configuration.Formatter.Test
{
    public class FormatterUnitTest
    {
        Dictionary<string, string> keyValuesTest;
        IConfiguration configuration;
        string json =
@"
{
  ""Key1"": ""ConfigValue1"",
  ""Key2"": ""ConfigValue2"",
  ""section1"": {
    ""Key3"": ""ConfigValue3""

  },
  ""section2"": {
    ""Key4"": ""ConfigValue4""
  },
  ""section3"": {
    ""dev"": {
      ""KeyU"": ""ConfigValueDEV""
    },
    ""prod"": {
      ""KeyU"": ""ConfigValuePROD""
    }
  },

  ""testconfigkey1"": ""{Key1}-{section2:Key4}"",
  ""testconfigkey2"": ""{Key1}-{Key2}-{MapKey4}"",
  ""testconfigkey3"": ""{Key2}-{Key4}-{MapKey4}-{KeyU}"",

}
";
        public FormatterUnitTest()
        {
            keyValuesTest = new Dictionary<string, string>
            {
                ["Key2"] = "MapValue2",

                ["MapKey1"] = "MapValue1",
                ["MapKey4"] = "MapValue4"
            };

            configuration = new ConfigurationBuilder().AddJsonStream(StreamHelpers.StringToStream(json)).Build();
        }

        [Fact]
        public void FormatKeyValueWithNameConfigurationRootOnly()
        {
            // Arrange 
            //"{Key1}-{section2:Key4}"
            var configKey = "testconfigkey1";
            var expected = "ConfigValue1-ConfigValue4";

            // Act
            var result = configuration.FormatKeyValue(configKey);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatKeyValueWithNameConfigurationDirRoot()
        {
            // Arrange 

            //"{Key1}-{Key2}-{MapKey4}"
            var configKey = "testconfigkey2";
            var expected = "ConfigValue1-MapValue2-MapValue4";

            // Act
            var result = configuration.FormatKeyValue(configKey, keyValueMap: keyValuesTest);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatKeyValueWithNameConfigurationSectionRoot()
        {
            // Arrange 

            //"{Key2}-{Key4}-{MapKey4}-{KeyU}"
            var configKey = "testconfigkey3";
            var expected = "MapValue2-ConfigValue4-MapValue4-ConfigValuePROD";
            var sections = new List<string> { "section2", "section3:prod" };

            // Act
            var result = configuration.FormatKeyValue(configKey, sectionSearchList: sections, keyValueMap: keyValuesTest);

            // Assert
            Assert.Equal(expected, result);
        }

    }
}
