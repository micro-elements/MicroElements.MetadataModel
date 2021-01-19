﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// ReadOnly initialized instance of <see cref="IXmlParserSettings"/>.
    /// </summary>
    public class XmlParserSettings : IXmlParserSettings
    {
        /// <inheritdoc/>
        public Func<XElement, string> GetElementName { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IParserRule> ParserRules { get; }

        /// <inheritdoc/>
        public IEqualityComparer<IProperty> PropertyComparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParserSettings"/> class.
        /// </summary>
        /// <param name="getElementName">Function that evaluates property name for xml element.</param>
        /// <param name="parserRules">Parsers and rules for parsers.</param>
        /// <param name="propertyComparer">Property comparer for property related search. Default value: <see cref="Metadata.PropertyComparer.ByReferenceComparer"/>.</param>
        public XmlParserSettings(
            Func<XElement, string>? getElementName = null,
            IReadOnlyCollection<IParserRule>? parserRules = null,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            GetElementName = getElementName ?? XmlParser.GetElementNameDefault;
            ParserRules = parserRules ?? XmlParser.CreateDefaultXmlParsersRules().ToArray();
            PropertyComparer = propertyComparer ?? Metadata.PropertyComparer.ByReferenceComparer;
        }
    }
}