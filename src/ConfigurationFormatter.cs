//   \\      /\  /\\
//  o \\ \  //\\// \\
//  |  \//\//       \\
// Copyright (c) i-Wallsmedia 2023. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Represents application configuration values wrapped by a formatter.
    /// </summary>
    public class ConfigurationFormatter : IConfiguration
    {

        private readonly IConfiguration _configuration;

        public HashSet<string> ConfigurationHashKeys { get; set; }

        /// <summary>
        /// Gets and sets the list of sections that used by a configuration formatter.
        /// </summary>
        public List<string> SectionList { get; set; }

        /// <summary>
        /// Gets and sets the key to new key mapping that used by a configuration formatter.
        /// </summary>
        public Dictionary<string, string> KeyValues { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configuration">The configuration section to wrap with a formatter.</param>
        public ConfigurationFormatter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets or sets a configuration value.
        /// The configuration value will be formatted.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value.</returns>
        public string this[string key]
        {
            get => _configuration.FormatKeyValue(key, SectionList, KeyValues, this);
            set => _configuration[key] = value;
        }

        /// <summary>
        /// Gets the immediate descendant configuration sub-sections.
        /// The children will be wrapped with <see cref="ConfigurationSectionFormatter"></see>
        /// </summary>
        /// <returns>The configuration sub-sections.</returns>
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _configuration.GetChildren().Select(s => s.UseSectionFormatter(_configuration, this, SectionList, KeyValues));
        }

        /// <summary>
        /// Returns a <see cref="IChangeToken"/> that can be used to observe when this configuration is reloaded.
        /// </summary>
        /// <returns>A <see cref="IChangeToken"/>.</returns>
        public IChangeToken GetReloadToken()
        {
            return _configuration.GetReloadToken();

        }

        /// <summary>
        /// Gets a configuration sub-section with the specified key.
        /// The section will be wrapped with <see cref="ConfigurationSectionFormatter"></see>
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <returns>The <see cref="IConfigurationSection"/>.</returns>
        /// <remarks>
        ///     This method will never return <c>null</c>. If no matching sub-section is found with the specified key,
        ///     an empty <see cref="IConfigurationSection"/> will be returned.
        /// </remarks>
        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key).UseSectionFormatter(_configuration, this, SectionList, KeyValues);
        }
    }
}
