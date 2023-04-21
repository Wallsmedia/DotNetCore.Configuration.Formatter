//   \\      /\  /\\
//  o \\ \  //\\// \\
//  |  \//\//       \\
// Copyright (c) i-Wallsmedia 2022. All rights reserved.

// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Represents a section of application configuration values wrapped by a formatter.
    /// </summary>
    public class ConfigurationSectionFormatter : IConfigurationSection
    {

        private readonly IConfigurationSection _section;
        public IConfiguration Root { get; }
        public ConfigurationFormatter ConfigurationFormatter { get; }


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
        /// <param name="section">The configuration section to wrap with a formatter.</param>
        /// <param name="root">The configuration root.</param>
        public ConfigurationSectionFormatter(IConfigurationSection section, IConfiguration root, ConfigurationFormatter configurationFormatter)
        {
            _section = section;
            Root = root;
            ConfigurationFormatter = configurationFormatter;
        }

        /// <summary>
        /// Gets or sets a configuration value.
        /// The configuration value will be formatted.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value.</returns>
        public string this[string key]
        {
            get
            {
                var val = _section[key];
                return FormatterExtensions.FormatValue(val, KeyValues, SectionList, Root, ConfigurationFormatter);
            }
            set => _section[key] = value;
        }

        /// <summary>
        /// Gets the immediate descendant configuration sub-sections.
        /// The children will be wrapped with <see cref="ConfigurationSectionFormatter"></see>
        /// </summary>
        /// <returns>The configuration sub-sections.</returns>
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _section.GetChildren().Select(s => s.UseSectionFormater(Root, ConfigurationFormatter, SectionList, KeyValues));
        }

        /// <summary>
        /// Returns a <see cref="IChangeToken"/> that can be used to observe when this configuration is reloaded.
        /// </summary>
        /// <returns>A <see cref="IChangeToken"/>.</returns>
        public IChangeToken GetReloadToken()
        {
            return _section.GetReloadToken();

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
            return _section.GetSection(key).UseSectionFormater(Root, ConfigurationFormatter, SectionList, KeyValues);
        }

        #region IConfigurationSection

        /// <summary>
        ///   Gets the key this section occupies in its parent.
        /// </summary>
        public string Key => _section.Key;

        /// <summary>
        /// Gets the full path to this section within the Microsoft.Extensions.Configuration.IConfiguration.
        /// </summary>
        public string Path => _section.Path;

        /// <summary>
        /// Gets or sets the section value.
        /// The Configuration formatter of root section will be applied while it gets value. 
        /// </summary>
        public string Value
        {
            get => FormatterExtensions.FormatValue(_section.Value, KeyValues, SectionList, Root, ConfigurationFormatter);
            set => _section.Value = value;
        }

        #endregion
    }
}
