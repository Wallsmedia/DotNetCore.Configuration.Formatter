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
    public class FormatStringUnitTest
    {
        readonly Dictionary<string, string> keyValuesTest;

        public FormatStringUnitTest()
        {
            keyValuesTest = new Dictionary<string, string>
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2",
                ["Key3"] = "Value3",
                ["Key4"] = "Value4",
                ["super-Value1-Value4"] = "SuperReplace"
            };
        }

        [Theory()]
        [InlineData("{Key1}", "Value1")]
        [InlineData("{{Key1}", "{Value1")]
        [InlineData("{{Key1}}", "{Value1}")]
        [InlineData("{{{Key1}", "{{Value1")]
        [InlineData("head_{Key1}", "head_Value1")]
        [InlineData("{Key1}_tail", "Value1_tail")]
        [InlineData("head_{Key1}_tail", "head_Value1_tail")]
        [InlineData("{Key}", "{Key}")]
        [InlineData("h{Key}", "h{Key}")]
        [InlineData("h{Key}t", "h{Key}t")]
        [InlineData("{Key}t", "{Key}t")]
        [InlineData("head_{Key1}{Key4}_tail", "head_Value1Value4_tail")]
        [InlineData("head_{Key1}{Key4}", "head_Value1Value4")]
        [InlineData("{Key1}{Key4}_tail", "Value1Value4_tail")]
        [InlineData("{Key1}{Key4}", "Value1Value4")]
        [InlineData("{Key1}-{Key4}", "Value1-Value4")]
        [InlineData("head_{Key}{Key4}_tail", "head_{Key}{Key4}_tail")]
        [InlineData("head_{Key}{Key4}", "head_{Key}{Key4}")]
        [InlineData("{Key}{Key4}_tail", "{Key}{Key4}_tail")]
        [InlineData("{Key}{Key4}", "{Key}{Key4}")]
        [InlineData("{Key}-{Key4}", "{Key}-{Key4}")]
        [InlineData("{KeyMe??defaulVal}-{Key4ME??defaulVal}", "defaulVal-defaulVal")]
        [InlineData("{super-{Key1}-{Key4}}", "SuperReplace")]
        [InlineData("{KeyMe??}", "")]

        public void TestFormatString(string source, string expected)
        {
            // Arrange 

            // Act
            var result = source.FormatString(keyValuesTest);

            // Assert

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestPairIndexes()
        {

            // Arrange 

            string test = "{super-{{toRep{dd}lace1}-{Key4}}}";

            // Act
            var result = FormatterExtensions.PairIndexes(test, '{', '}');

            // Assert

            Assert.Equal((14, 17), result);
        }
    }
}
