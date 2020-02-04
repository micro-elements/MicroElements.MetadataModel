﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents metadata for property.
    /// Every object consist of many properties and we should map properties from different sources to one common model.
    /// </summary>
    public interface IProperty : IMetadataProvider
    {
        /// <summary>
        /// Unique property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Property value type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Property description.
        /// </summary>
        LocalizableString Description { get; }

        /// <summary>
        /// Alternative code for property.
        /// </summary>
        string Alias { get; }
    }

    /// <summary>
    /// Strong typed property description.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IProperty<out T> : IProperty
    {
        /// <summary>
        /// Gets default value for property.
        /// </summary>
        Func<T> DefaultValue { get; }

        /// <summary>
        /// Gets examples list.
        /// </summary>
        IReadOnlyList<T> Examples { get; }

        /// <summary>
        /// Gets Calculate func for calculated properties.
        /// </summary>
        Func<IPropertyContainer, T> Calculate { get; }
    }
}
