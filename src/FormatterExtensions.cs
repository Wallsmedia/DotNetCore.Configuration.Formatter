//   \\      /\  /\\
//  o \\ \  //\\// \\
//  |  \//\//       \\
// Copyright (c) i-Wallsmedia 2022. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

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
        ///     var keyValues = new Dictionary&gt;string, string>()
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
            return (T)GetFormatted(configuration, typeof(T), typeof(T).Name);
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
        /// <param name="configuration">The configuration instance.</param>
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
                var defaultValue = string.Empty;
                bool useDefaults = valueKeyName.Contains("??");
                var defaults = valueKeyName.Split(new string[] { "??" }, System.StringSplitOptions.RemoveEmptyEntries);
                string encodeKeyName;
                if (defaults.Length > 1)
                {
                    encodeKeyName = defaults[0];
                    defaultValue = defaults[1];
                }
                else
                {
                    encodeKeyName = valueKeyName;
                }

                var kevalue = KeyValue(encodeKeyName, keyValueMap, sectionSearchList, configuration);

                if (kevalue != null)
                {
                    // the key value has been resolved 
                    sb.Append(kevalue);
                }
                else if (useDefaults)
                {
                    sb.Append(defaultValue);
                }
                else
                {
                    // not found 
                    sb.Append('{');
                    sb.Append(valueKeyName);
                    sb.Append('}');
                    complete = true;
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

        /// <summary>
        /// Resolve the key value of the configuration.
        /// Gets configuration value by a key, formats it and stores back.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="key">The configuration key</param>
        /// <returns>The operation result.True if a key value has been changed.</returns>
        public static bool ResolveKeyValue(this IConfiguration configuration, string key)
        {
            var oldValue = configuration[key];
            var newValue = FormatValue(oldValue, null, null, configuration);
            configuration[key] = newValue;
            newValue = configuration[key];
            return oldValue != newValue;
        }

        /// <summary>
        /// Resolve all keys value of the configuration.
        /// Gets configuration values by keys, formats it and stores back.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns>True if keys values have might been changed.</returns>
        public static bool ResolveAllKeyValues(this IConfiguration configuration)
        {
            var rootConfiguration = configuration as IConfigurationRoot;
            if (rootConfiguration != null)
            {
                var keys = rootConfiguration.AllConfigurationKeys();
                foreach (var key in keys)
                {
                    var oldValue = configuration[key];
                    if (oldValue != null && oldValue.IndexOf('{') != -1)
                    {
                        configuration[key] = FormatValue(oldValue, null, null, configuration);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a list of all Configuration keys
        /// </summary>
        /// <param name="root">The root configuration instance.</param>
        /// <returns>List of all keys.</returns>
        public static List<string> AllConfigurationKeys(this IConfigurationRoot root)
        {
            (string Value, IConfigurationProvider Provider) GetValueAndProvider(IConfigurationRoot rootF, string key)
            {
                foreach (IConfigurationProvider provider in rootF.Providers.Reverse())
                {
                    var probe = provider.TryGet(key, out string value);
                    if (probe || value != null)
                    {
                        return (value, provider);
                    }
                }
                return (null, null);
            }
            void RecurseChildren(HashSet<string> keysF, IEnumerable<IConfigurationSection> children, string rootPath)
            {
                foreach (IConfigurationSection child in children)
                {
                    (string Value, IConfigurationProvider Provider) valueAndProvider = GetValueAndProvider(root, child.Path);
                    if (valueAndProvider.Provider != null)
                    {
                        if (string.IsNullOrEmpty(rootPath))
                        {
                            keysF.Add(child.Key);
                        }
                        else
                        {
                            keysF.Add(rootPath + ":" + child.Key);
                        }
                    }
                    RecurseChildren(keysF, child.GetChildren(), child.Path);
                }
            }
            var keys = new HashSet<string>();
            RecurseChildren(keys, root.GetChildren(), "");
            return keys.ToList();
        }

        /// <summary>
        /// Verify that key exist even it is an empty string 
        /// </summary>
        /// <param name="root">The root configuration instance.</param>
        /// <param name="complexKey">The probed key.</param>
        /// <returns>Returns true if it exist.</returns>
        public static bool ContainsComplexKey(this IConfigurationRoot root, string complexKey)
        {
            var list = root.AllConfigurationKeys();
            return list.Any(k => k.ToLower() == complexKey.ToLower());
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
            string result = null;

            if (configuration != null)
            {
                result = configuration[keyName];
                if (result == null && configuration as IConfigurationRoot != null)
                {
                    if (((IConfigurationRoot)configuration).ContainsComplexKey(keyName))
                    {
                        result = string.Empty;
                    }
                }
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
