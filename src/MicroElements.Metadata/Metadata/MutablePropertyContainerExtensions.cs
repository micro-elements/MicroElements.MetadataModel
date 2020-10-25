﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static class MutablePropertyContainerExtensions
    {
        /// <summary>
        /// Sets parent property source and returns the same changed propertyContainer.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <returns>The same container with changed parent.</returns>
        public static IMutablePropertyContainer WithParentPropertySource(this IMutablePropertyContainer propertyContainer, IPropertyContainer parentPropertySource)
        {
            propertyContainer.SetParentPropertySource(parentPropertySource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets property value and returns the same container.
        /// </summary>
        /// <typeparam name="TContainer">Property container type.</typeparam>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>The same container with changed property.</returns>
        public static TContainer WithValue<TContainer, T>(this TContainer propertyContainer, IProperty<T> property, T value, ValueSource? valueSource = default)
            where TContainer : IMutablePropertyContainer
        {
            propertyContainer.SetValue(property, value, valueSource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets value by string property name and returns the same container.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <typeparam name="TContainer">Property container type.</typeparam>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>The same container with changed property.</returns>
        public static TContainer WithValue<TContainer, T>(this TContainer propertyContainer, string propertyName, T value, ValueSource? valueSource = default)
            where TContainer : IMutablePropertyContainer
        {
            propertyContainer.SetValue(propertyName, value, valueSource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets property value and returns the same container.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValueUntyped(this IMutablePropertyContainer propertyContainer, IProperty property, object value, ValueSource valueSource = default)
        {
            propertyContainer.SetValueUntyped(property, value, valueSource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets value by string property name.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public static IPropertyValue<T> SetValue<T>(this IMutablePropertyContainer propertyContainer, string propertyName, T value, ValueSource valueSource = default)
        {
            IPropertyValue propertyValue = propertyContainer.GetPropertyValueUntyped(Search
                .ByNameOrAlias<T>(propertyName, true)
                .SearchInParent(false)
                .ReturnNull());

            if (propertyValue != null)
            {
                Type valueType = typeof(T);
                IProperty existingProperty = propertyValue.PropertyUntyped;
                if (existingProperty.Type != valueType)
                {
                    throw new ArgumentException($"Existing property {existingProperty.Name} has type {existingProperty.Type} but value has type {valueType}");
                }

                return propertyContainer.SetValue((IProperty<T>)existingProperty, value, valueSource);
            }

            return propertyContainer.SetValue(new Property<T>(propertyName), value, valueSource);
        }

        /// <summary>
        /// Sets value by string property name.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <param name="valueType">Value type if value is null.</param>
        /// <returns><see cref="IPropertyValue"/> that holds value for property.</returns>
        public static IPropertyValue SetValueUntyped(this IMutablePropertyContainer propertyContainer, string propertyName, object value, ValueSource valueSource = default, Type valueType = null)
        {
            IPropertyValue propertyValue = propertyContainer.GetPropertyValueUntyped(Search
                .ByNameOrAlias(propertyName, true)
                .SearchInParent(false)
                .ReturnNull());

            if (propertyValue != null)
            {
                IProperty existingProperty = propertyValue.PropertyUntyped;

                if (value != null && value.GetType() != existingProperty.Type)
                {
                    throw new ArgumentException($"Existing property {existingProperty.Name} has type {existingProperty.Type} but value has type {value.GetType()}");
                }

                if (value == null && existingProperty.Type.IsValueType)
                {
                    throw new ArgumentException($"Existing property {existingProperty.Name} has type {existingProperty.Type} and null value is not allowed");
                }

                return propertyContainer.SetValueUntyped(existingProperty, value, valueSource);
            }
            else
            {
                if (value == null && valueType == null)
                    throw new InvalidOperationException($"Unable to define property type for {propertyName} because value is null");

                Type propertyTypeByValue = value?.GetType() ?? valueType;
                IProperty newProperty = Property.Create(propertyTypeByValue, propertyName);
                return propertyContainer.SetValueUntyped(newProperty, value, valueSource);
            }
        }

        /// <summary>
        /// Sets property and value (non generic version).
        /// </summary>
        /// <param name="propertyContainer">PropertyContainer to change.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue"/> that holds value for property.</returns>
        public static IPropertyValue SetValueUntyped(this IMutablePropertyContainer propertyContainer, IProperty property, object value, ValueSource valueSource = default)
        {
            IPropertyValue propertyValue = PropertyValue.Create(property, value, valueSource);
            propertyContainer.SetValue(propertyValue);
            return propertyValue;
        }

        /// <summary>
        /// Adds property values.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="propertyValues">PropertyValue list.</param>
        public static void AddRange(this IMutablePropertyContainer propertyContainer, IEnumerable<IPropertyValue> propertyValues)
        {
            foreach (IPropertyValue propertyValue in propertyValues)
            {
                propertyContainer.Add(propertyValue);
            }
        }

        /// <summary>
        /// Sets property value if property is not set.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValueIfNotSet<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, T value)
        {
            var propertyValue = propertyContainer.GetPropertyValueUntyped(property, SearchOptions.ExistingOnly);
            if (propertyValue.IsNullOrNotDefined())
                propertyContainer.SetValue(property, value);
        }

        /// <summary>
        /// Sets optional value if <paramref name="value"/> is in Some state.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValue<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, in Option<T> value)
        {
            value.Match(val => propertyContainer.SetValue(property, val), () => { });
        }

        /// <summary>
        /// Sets optional value if <paramref name="value"/> is in Some state.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">Property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetValue<T>(this IMutablePropertyContainer propertyContainer, IProperty<T?> property, in Option<T> value)
            where T : struct
        {
            value.Match(val => propertyContainer.SetValue(property, val), () => { });
        }
    }
}
