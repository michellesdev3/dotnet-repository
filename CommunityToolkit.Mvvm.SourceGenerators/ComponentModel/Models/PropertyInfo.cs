// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Helpers;

namespace CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;

/// <summary>
/// A model representing an generated property
/// </summary>
/// <param name="TypeName">The type name for the generated property.</param>
/// <param name="IsNullableReferenceType">Whether or not the property is of a nullable reference type.</param>
/// <param name="FieldName">The field name.</param>
/// <param name="PropertyName">The generated property name.</param>
/// <param name="PropertyChangingNames">The sequence of property changing properties to notify.</param>
/// <param name="PropertyChangedNames">The sequence of property changed properties to notify.</param>
/// <param name="NotifiedCommandNames">The sequence of commands to notify.</param>
/// <param name="AlsoBroadcastChange">Whether or not the generated property also broadcasts changes.</param>
/// <param name="ValidationAttributes">The sequence of validation attributes for the generated property.</param>
internal sealed record PropertyInfo(
    string TypeName,
    bool IsNullableReferenceType,
    string FieldName,
    string PropertyName,
    ImmutableArray<string> PropertyChangingNames,
    ImmutableArray<string> PropertyChangedNames,
    ImmutableArray<string> NotifiedCommandNames,
    bool AlsoBroadcastChange,
    ImmutableArray<AttributeInfo> ValidationAttributes)
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="PropertyInfo"/>.
    /// </summary>
    public sealed class Comparer : Comparer<PropertyInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, PropertyInfo obj)
        {
            hashCode.Add(obj.TypeName);
            hashCode.Add(obj.IsNullableReferenceType);
            hashCode.Add(obj.FieldName);
            hashCode.Add(obj.PropertyName);
            hashCode.AddRange(obj.PropertyChangingNames);
            hashCode.AddRange(obj.PropertyChangedNames);
            hashCode.AddRange(obj.NotifiedCommandNames);
            hashCode.Add(obj.AlsoBroadcastChange);
            hashCode.AddRange(obj.ValidationAttributes, AttributeInfo.Comparer.Default);
        }

        /// <inheritdoc/>
        protected override bool AreEqual(PropertyInfo x, PropertyInfo y)
        {
            return
                x.TypeName == y.TypeName &&
                x.IsNullableReferenceType == y.IsNullableReferenceType &&
                x.FieldName == y.FieldName &&
                x.PropertyName == y.PropertyName &&
                x.PropertyChangingNames.SequenceEqual(y.PropertyChangingNames) &&
                x.PropertyChangedNames.SequenceEqual(y.PropertyChangedNames) &&
                x.NotifiedCommandNames.SequenceEqual(y.NotifiedCommandNames) &&
                x.AlsoBroadcastChange == y.AlsoBroadcastChange &&
                x.ValidationAttributes.SequenceEqual(y.ValidationAttributes, AttributeInfo.Comparer.Default);
        }
    }
}
