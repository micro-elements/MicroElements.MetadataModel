﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Represents maximum allowed length for string values.
    /// It's an equivalent of JsonSchema maxLength.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IStringMaxLength : IMetadata
    {
        /// <summary>
        /// Gets maximum allowed length for string values.
        /// </summary>
        int? MaxLength { get; }
    }

    /// <inheritdoc/>
    public sealed class StringMaxLength : IStringMaxLength
    {
        /// <inheritdoc/>
        public int? MaxLength { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMaxLength"/> class.
        /// </summary>
        /// <param name="maxLength">Maximum allowed length for string values.</param>
        /// <exception cref="ArgumentException">MaxLength should be a non-negative integer.</exception>
        public StringMaxLength(int? maxLength)
        {
            if (maxLength.HasValue && maxLength.Value < 0)
                throw new ArgumentException($"MaxLength should be a non-negative integer but was {maxLength}");
            MaxLength = maxLength;
        }
    }
}
