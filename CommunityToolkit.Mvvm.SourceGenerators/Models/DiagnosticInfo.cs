// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file is ported and adapted from ComputeSharp (Sergio0694/ComputeSharp),
// more info in ThirdPartyNotices.txt in the root of the project.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CommunityToolkit.Mvvm.SourceGenerators.Models;

/// <summary>
/// A model for a serializeable diagnostic info.
/// </summary>
/// <param name="Descriptor">The wrapped <see cref="DiagnosticDescriptor"/> instance.</param>
/// <param name="SyntaxTree">The tree to use as location for the diagnostic, if available.</param>
/// <param name="TextSpan">The span to use as location for the diagnostic.</param>
/// <param name="Arguments">The diagnostic arguments.</param>
internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    SyntaxTree? SyntaxTree,
    TextSpan TextSpan,
    ImmutableArray<string> Arguments)
{
    /// <inheritdoc/>
    public bool Equals(DiagnosticInfo? obj) => Comparer.Default.Equals(this, obj);

    /// <inheritdoc/>
    public override int GetHashCode() => Comparer.Default.GetHashCode(this);

    /// <summary>
    /// Creates a new <see cref="Diagnostic"/> instance with the state from this model.
    /// </summary>
    /// <returns>A new <see cref="Diagnostic"/> instance with the state from this model.</returns>
    public Diagnostic ToDiagnostic()
    {
        if (SyntaxTree is not null)
        {
            return Diagnostic.Create(Descriptor, Location.Create(SyntaxTree, TextSpan), Arguments.ToArray());
        }

        return Diagnostic.Create(Descriptor, null, Arguments.ToArray());
    }

    /// <summary>
    /// Creates a new <see cref="DiagnosticInfo"/> instance with the specified parameters.
    /// </summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="symbol">The source <see cref="ISymbol"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    /// <returns>A new <see cref="DiagnosticInfo"/> instance with the specified parameters.</returns>
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol symbol, params object[] args)
    {
        Location location = symbol.Locations.First();

        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableArray());
    }

    /// <summary>
    /// Creates a new <see cref="DiagnosticInfo"/> instance with the specified parameters.
    /// </summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="node">The source <see cref="SyntaxNode"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    /// <returns>A new <see cref="DiagnosticInfo"/> instance with the specified parameters.</returns>
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode node, params object[] args)
    {
        Location location = node.GetLocation();

        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableArray());
    }

    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="DiagnosticInfo"/>.
    /// </summary>
    private sealed class Comparer : Comparer<DiagnosticInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, DiagnosticInfo obj)
        {
            hashCode.Add(obj.Descriptor);
            hashCode.Add(obj.SyntaxTree);
            hashCode.Add(obj.TextSpan);
            hashCode.AddRange(obj.Arguments);
        }

        /// <inheritdoc/>
        protected override bool AreEqual(DiagnosticInfo x, DiagnosticInfo y)
        {
            return
                x.Descriptor.Equals(y.Descriptor) &&
                x.SyntaxTree == y.SyntaxTree &&
                x.TextSpan.Equals(y.TextSpan) &&
                x.Arguments.SequenceEqual(y.Arguments);
        }
    }
}
