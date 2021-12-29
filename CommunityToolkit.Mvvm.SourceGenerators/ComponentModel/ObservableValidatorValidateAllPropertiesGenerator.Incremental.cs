// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.SourceGenerators.Input.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CommunityToolkit.Mvvm.SourceGenerators;

/// <summary>
/// A source generator for message registration without relying on compiled LINQ expressions.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ObservableValidatorValidateAllPropertiesGenerator2 : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Validate the language version (this needs at least C# 8.0 due to static local functions being used).
        // If a lower C# version is set, just skip the execution silently. The fallback path will be used just fine.
        IncrementalValueProvider<bool> isGeneratorSupported =
            context.ParseOptionsProvider
            .Select(static (item, _) => item is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp8 });

        // Get all class declarations
        IncrementalValuesProvider<INamedTypeSymbol> typeSymbols =
            context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!)
            .Combine(isGeneratorSupported)
            .Where(static item => item.Right)
            .Select(static (item, _) => item.Left);

        // Get the types that inherit from ObservableValidator and gather their info
        IncrementalValuesProvider<ValidationInfo> validationInfo =
            typeSymbols
            .Where(Execute.IsObservableValidator)
            .Select(static (item, _) => Execute.GetInfo(item))
            .WithComparer(ValidationInfo.Comparer.Default);

        // Check whether the header file is needed
        IncrementalValueProvider<bool> isHeaderFileNeeded =
            validationInfo
            .Collect()
            .Select(static (item, _) => item.Length > 0);

        // Generate the header file with the attributes
        context.RegisterImplementationSourceOutput(isHeaderFileNeeded, static (context, item) =>
        {
            CompilationUnitSyntax compilationUnit = Execute.GetSyntax();

            context.AddSource(
                hintName: "__ObservableValidatorExtensions.cs",
                sourceText: SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
        });

        // Generate the class with all validation methods
        context.RegisterImplementationSourceOutput(validationInfo, static (context, item) =>
        {
            CompilationUnitSyntax compilationUnit = Execute.GetSyntax(item);

            context.AddSource(
                hintName: $"{item.FilenameHint}.cs",
                sourceText: SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
        });
    }
}
