﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// XmlParser mutable context.
    /// </summary>
    public interface IXmlParserContext
    {
        /// <summary>
        /// Gets <see cref="IXmlParserSettings"/> for this context.
        /// </summary>
        IXmlParserSettings ParserSettings { get; }

        /// <summary>
        /// Gets messages list.
        /// </summary>
        IMutableMessageList<Message> Messages { get; }

        /// <summary>
        /// Gets parsers cache.
        /// </summary>
        ConcurrentDictionary<IProperty, IValueParser> ParsersCache { get; }

        /// <summary>
        /// Gets schema cache.
        /// </summary>
        ConcurrentDictionary<IProperty, ISchema> SchemaCache { get; }
    }

    /// <summary>
    /// XmlParser mutable context.
    /// </summary>
    public class XmlParserContext : IXmlParserContext
    {
        /// <inheritdoc />
        public IXmlParserSettings ParserSettings { get; }

        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<IProperty, IValueParser> ParsersCache { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<IProperty, ISchema> SchemaCache { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParserContext"/> class.
        /// </summary>
        /// <param name="parserSettings">Parser settings.</param>
        /// <param name="messages">Optional message list.</param>
        /// <param name="parsersCache">Optional parsers cache.</param>
        /// <param name="schemaCache">Optional schemas cache.</param>
        public XmlParserContext(
            IXmlParserSettings parserSettings,
            IMutableMessageList<Message>? messages = null,
            ConcurrentDictionary<IProperty, IValueParser>? parsersCache = null,
            ConcurrentDictionary<IProperty, ISchema>? schemaCache = null)
        {
            parserSettings.AssertArgumentNotNull(nameof(parserSettings));

            ParserSettings = parserSettings;
            Messages = messages ?? new MutableMessageList<Message>();
            ParsersCache = parsersCache ?? new ConcurrentDictionary<IProperty, IValueParser>(comparer: parserSettings.PropertyComparer);
            SchemaCache = schemaCache ?? new ConcurrentDictionary<IProperty, ISchema>(comparer: parserSettings.PropertyComparer);
        }
    }
}
