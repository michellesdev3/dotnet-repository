﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Toolkit.Mvvm.Input;

namespace Microsoft.Toolkit.Mvvm.SourceGenerators
{
    /// <inheritdoc cref="ICommandGenerator"/>
    public sealed partial class ICommandGenerator
    {
        /// <summary>
        /// An <see cref="ISyntaxContextReceiver"/> that selects candidate nodes to process.
        /// </summary>
        private sealed class SyntaxReceiver : ISyntaxContextReceiver
        {
            /// <summary>
            /// The list of info gathered during exploration.
            /// </summary>
            private readonly List<IMethodSymbol> gatheredInfo = new();

            /// <summary>
            /// Gets the collection of gathered info to process.
            /// </summary>
            public IReadOnlyCollection<IMethodSymbol> GatheredInfo => this.gatheredInfo;

            /// <inheritdoc/>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                    context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol &&
                    context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(ICommandAttribute).FullName) is INamedTypeSymbol iCommandSymbol &&
                    methodSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, iCommandSymbol)))
                {
                    this.gatheredInfo.Add(methodSymbol);
                }
            }
        }
    }
}
