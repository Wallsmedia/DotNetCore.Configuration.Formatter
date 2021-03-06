//   \\      /\  /\\
//  o \\ \  //\\// \\
//  |  \//\//       \\
// Copyright (c) i-Wallsmedia 2020. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Collections.Generic;
using System;

namespace Microsoft.Extensions.Configuration
{

    /// <summary>
    /// Formatter to substitutes the named format items like {NamedformatItem} format  with values from IConfiguration or dictionary.
    /// </summary>
    public static class FormatterExtensions
    {
        /// <summary>
        /// The string formatting extension. Formats string by substituting  format expression with key values from the map. 
        /// </summary>
        /// <param name="value">A string to format.</param>
        /// <param name="keyValues">A key value map.</param>
        /// <example>
        ///     var keyValues = new Dictionary<string, string>()
        ///     {
        ///         ["Key-1"] = "Value-1",
        ///         ["Key-Value-1"] = "Complex-Value-1"
        ///     };
        ///     var format = "Get the {Key-1} and complex: {Key-{Key-1}} and {none??Default}";
        ///     var formatted = format.FormatString(keyValues);
        /// 
        ///    Formated string will be 
        ///        "Get the Value-1 and complex: Complex-Value-1 and Default"
        /// </example>
        /// <returns>The formatted string.</returns>
        public static string FormatString(this string value, Dictionary<string, string> keyValues)
        {
            return FormatValue(value, keyValues, null, null);
        }


        /// <summary>
        /// Attempts to bind the configuration instance to a new instance of type T.
        /// </summary>
        /// <typeparam name="T">The type of the new instance to bind and the section name.</typeparam>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <returns>The new instance if successful, null otherwise.</returns>
        public static T GetTypeNameFormatted<T>(this IConfiguration configuration)
        {
            return (T)GetFormatted(configuration, typeof(T), nameof(T));
        }

        /// <summary>
        /// Attempts to bind the configuration instance to a new instance of type T.
        /// If this configuration section has a value, that will be used.
        /// Otherwise binding by matching property names against configuration keys recursively.
        /// </summary>
        /// <typeparam name="T">The type of the new instance to bind.</typeparam>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="section">The configuration section.</param>
        /// <returns>The new instance if successful, null otherwise.</returns>
        public static T GetFormatted<T>(this IConfiguration configuration, string section = null)
        {
            return (T)GetFormatted(configuration, typeof(T), section);
        }

        /// <summary>
        /// Attempts to bind the configuration instance to a new instance of type T.
        /// If this configuration section has a value, that will be used.
        /// Otherwise binding by matching property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="type">The type of the new instance to bind.</param>
        /// <param name="section">The configuration section.</param>
        /// <returns>The new instance if successful, null otherwise.</returns>
        public static object GetFormatted(this IConfiguration configuration, Type type, string section)
        {
            var wrapped = configuration.UseFormater();
            if (section != null)
            {
                return wrapped.GetSection(section).Get(type);
            }
            return wrapped.Get(type);
        }

        /// <summary>
        /// Wraps <see cref="IConfiguration"/> with <see cref="ConfigurationFormatter"/>.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="sectionSearchList">The list of sections for key search.</param>
        /// <param name="keyValueMap">The key to value map used for formating.</param>
        /// <returns>The wrapped configuration.</returns>
        public static ConfigurationFormatter UseFormater(this IConfiguration configuration, List<string> sectionSearchList = null, Dictionary<string, string> keyValueMap = null)
        {
            return new ConfigurationFormatter(configuration) { SectionList = sectionSearchList, KeyValues = keyValueMap };
        }

        /// <summary>
        /// Wraps <see cref="IConfigurationSection"/> with <see cref="ConfigurationSectionFormatter"/>.
        /// </summary>
        /// <param name="configurationSection">The configuration section instance.</param>
        /// <param name="sectionSearchList">The list of sections for key search.</param>
        /// <param name="keyValueMap">The key to value map used for formating.</param>
        /// <returns>The wrapped configuration section.</returns>
        public static ConfigurationSectionFormatter UseFormater(this IConfigurationSection configurationSection, IConfiguration configuration, List<string> sectionSearchList = null, Dictionary<string, string> keyValueMap = null)
        {
            return new ConfigurationSectionFormatter(configurationSection, configuration) { SectionList = sectionSearchList, KeyValues = keyValueMap };
        }

        /// <summary>
        /// Formats the key value of the configuration.
        /// Gets configuration value by a key and formats it.
        /// Order of resolution:
        /// - key value map
        ///  - sectionList
        /// -  root of configuration
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="key">The configuration key</param>
        /// <param name="sectionSearchList">The list of sections for key search.</param>
        /// <param name="keyValueMap">The map foe mapping keys before resolving.</param>
        /// <returns>The wrapped configuration section.</returns>
        public static string FormatKeyValue(this IConfiguration configuration, string key, List<string> sectionSearchList = null, Dictionary<string, string> keyValueMap = null)
        {
            var value = configuration[key];

            return FormatValue(value, keyValueMap, sectionSearchList, configuration);
        }

        /// <summary>
        /// Formats the value with using the configuration to format.
        /// 
        /// Order of resolution:
        /// - key value map
        ///  - sectionList
        /// -  root of configuration
        /// </summary>
        /// <param name="value">The string to format.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="sectionSearchList">The list of sections for key search.</param>
        /// <param name="keyValueMap">The key to value map used for formating.</param>
        /// <returns>The wrapped configuration section.</returns>
        public static string FormatValue(string value, Dictionary<string, string> keyValueMap, List<string> sectionSearchList = null, IConfiguration configuration = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var sb = new StringBuilder();
            bool complete = false;
            do
            {
                int openBraceIndex;
                int closeBraceIndex;
                (openBraceIndex, closeBraceIndex) = PairIndexes(value, '{', '}');

                if (openBraceIndex == 0 && closeBraceIndex == 0)
                {
                    complete = true;
                    continue;
                }
                sb.Append(value, 0, openBraceIndex);

                var valueKeyName = value.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1);
                var defaults = valueKeyName.Split("??", System.StringSplitOptions.RemoveEmptyEntries);
                string encodeKeyName;
                if (defaults.Length > 1)
                {
                    encodeKeyName = defaults[0];
                }
                else
                {
                    encodeKeyName = valueKeyName;
                }

                var kevalue = KeyValue(encodeKeyName, keyValueMap, sectionSearchList, configuration);

                if (!string.IsNullOrEmpty(kevalue))
                {
                    sb.Append(kevalue);
                }
                else if (defaults.Length > 1)
                {
                    sb.Append(defaults[1]);
                }
                else
                {
                    sb.Append('?');
                    sb.Append(valueKeyName);
                    sb.Append('?');
                }
                if (closeBraceIndex + 1 < value.Length)
                {
                    sb.Append(value, closeBraceIndex + 1, value.Length - closeBraceIndex - 1);
                }
                value = sb.ToString();
                sb.Clear();
            } while (!complete);

            return value;
        }

        private static string KeyValue(string keyName, Dictionary<string, string> keyValueMap, List<string> sectionList = null, IConfiguration configuration = null)
        {
            //
            // Order of resolution
            // key values
            // sectionList
            // root of configuration
            //
            if (keyValueMap != null)
            {
                if (keyValueMap.TryGetValue(keyName, out string value))
                {
                    return value;
                }
            }

            if (sectionList != null)
            {
                foreach (var section in sectionList)
                {
                    var value = configuration.GetSection(section)[keyName];
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            var result = string.Empty;

            if (configuration != null)
            {
                result = configuration[keyName];
            }

            return result;
        }

        public static (int, int) PairIndexes(string format, char left, char right)
        {
            Stack<int> open = new Stack<int>();
            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == left)
                {
                    open.Push(i);
                    continue;
                }
                if (format[i] == right)
                {
                    if (open.Count == 0)
                    {
                        break;
                    }
                    return (open.Pop(), i);
                }
            }
            return (0, 0);
        }
    }
}
