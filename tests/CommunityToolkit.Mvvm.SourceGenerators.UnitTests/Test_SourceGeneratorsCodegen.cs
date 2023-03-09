// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunityToolkit.Mvvm.SourceGenerators.UnitTests;

[TestClass]
public class Test_SourceGeneratorsCodegen
{
    [TestMethod]
    public void ObservablePropertyWithPartialMethodWithPreviousValuesNotUsed_DoesNotGenerateFieldReadAndMarksOldValueAsNullable()
    {
        string source = """
            using System.ComponentModel;
            using CommunityToolkit.Mvvm.ComponentModel;

            #nullable enable

            namespace MyApp;
            
            partial class MyViewModel : ObservableObject
            {
                [ObservableProperty]
                private string name = null!;
            }
            """;

        string result = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <inheritdoc cref="name"/>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    public string Name
                    {
                        get => name;
                        set
                        {
                            if (!global::System.Collections.Generic.EqualityComparer<string>.Default.Equals(name, value))
                            {
                                OnNameChanging(value);
                                OnNameChanging(default, value);
                                OnPropertyChanging(global::CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.Name);
                                name = value;
                                OnNameChanged(value);
                                OnNameChanged(default, value);
                                OnPropertyChanged(global::CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Name);
                            }
                        }
                    }

                    /// <summary>Executes the logic for when <see cref="Name"/> is changing.</summary>
                    /// <param name="value">The new property value being set.</param>
                    /// <remarks>This method is invoked right before the value of <see cref="Name"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnNameChanging(string value);
                    /// <summary>Executes the logic for when <see cref="Name"/> is changing.</summary>
                    /// <param name="oldValue">The previous property value that is being replaced.</param>
                    /// <param name="newValue">The new property value being set.</param>
                    /// <remarks>This method is invoked right before the value of <see cref="Name"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnNameChanging(string? oldValue, string newValue);
                    /// <summary>Executes the logic for when <see cref="Name"/> just changed.</summary>
                    /// <param name="value">The new property value that was set.</param>
                    /// <remarks>This method is invoked right after the value of <see cref="Name"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnNameChanged(string value);
                    /// <summary>Executes the logic for when <see cref="Name"/> just changed.</summary>
                    /// <param name="oldValue">The previous property value that was replaced.</param>
                    /// <param name="newValue">The new property value that was set.</param>
                    /// <remarks>This method is invoked right after the value of <see cref="Name"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnNameChanged(string? oldValue, string newValue);
                }
            }
            """;

        VerifyGenerateSources(source, new[] { new ObservablePropertyGenerator() }, ("MyApp.MyViewModel.g.cs", result));
    }

    // See https://github.com/CommunityToolkit/dotnet/issues/601
    [TestMethod]
    public void ObservablePropertyWithForwardedAttributesWithNumberLiterals_PreservesType()
    {
        string source = """
            using System.ComponentModel;
            using CommunityToolkit.Mvvm.ComponentModel;

            #nullable enable

            namespace MyApp;
            
            partial class MyViewModel : ObservableObject
            {
                const double MyDouble = 3.14;
                const float MyFloat = 3.14f;

                [ObservableProperty]
                [property: DefaultValue(0.0)]
                [property: DefaultValue(1.24)]
                [property: DefaultValue(0.0f)]
                [property: DefaultValue(0.0f)]
                [property: DefaultValue(MyDouble)]
                [property: DefaultValue(MyFloat)]
                private object? a;
            }

            public class DefaultValueAttribute : Attribute
            {
                public DefaultValueAttribute(object value)
                {
                }
            }
            """;

        string result = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <inheritdoc cref="a"/>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    [global::MyApp.DefaultValueAttribute(0D)]
                    [global::MyApp.DefaultValueAttribute(1.24D)]
                    [global::MyApp.DefaultValueAttribute(0F)]
                    [global::MyApp.DefaultValueAttribute(0F)]
                    [global::MyApp.DefaultValueAttribute(3.14D)]
                    [global::MyApp.DefaultValueAttribute(3.14F)]
                    public object? A
                    {
                        get => a;
                        set
                        {
                            if (!global::System.Collections.Generic.EqualityComparer<object?>.Default.Equals(a, value))
                            {
                                OnAChanging(value);
                                OnAChanging(default, value);
                                OnPropertyChanging(global::CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.A);
                                a = value;
                                OnAChanged(value);
                                OnAChanged(default, value);
                                OnPropertyChanged(global::CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.A);
                            }
                        }
                    }

                    /// <summary>Executes the logic for when <see cref="A"/> is changing.</summary>
                    /// <param name="value">The new property value being set.</param>
                    /// <remarks>This method is invoked right before the value of <see cref="A"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnAChanging(object? value);
                    /// <summary>Executes the logic for when <see cref="A"/> is changing.</summary>
                    /// <param name="oldValue">The previous property value that is being replaced.</param>
                    /// <param name="newValue">The new property value being set.</param>
                    /// <remarks>This method is invoked right before the value of <see cref="A"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnAChanging(object? oldValue, object? newValue);
                    /// <summary>Executes the logic for when <see cref="A"/> just changed.</summary>
                    /// <param name="value">The new property value that was set.</param>
                    /// <remarks>This method is invoked right after the value of <see cref="A"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnAChanged(object? value);
                    /// <summary>Executes the logic for when <see cref="A"/> just changed.</summary>
                    /// <param name="oldValue">The previous property value that was replaced.</param>
                    /// <param name="newValue">The new property value that was set.</param>
                    /// <remarks>This method is invoked right after the value of <see cref="A"/> is changed.</remarks>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                    partial void OnAChanged(object? oldValue, object? newValue);
                }
            }
            """;

        VerifyGenerateSources(source, new[] { new ObservablePropertyGenerator() }, ("MyApp.MyViewModel.g.cs", result));
    }

    [TestMethod]
    public void RelayCommandMethodWithForwardedAttributesWithNumberLiterals_PreservesType()
    {
        string source = """
            using CommunityToolkit.Mvvm.Input;

            #nullable enable

            namespace MyApp;
            
            partial class MyViewModel
            {
                const double MyDouble = 3.14;
                const float MyFloat = 3.14f;

                [RelayCommand]
                [field: DefaultValue(0.0)]
                [field: DefaultValue(1.24)]
                [field: DefaultValue(0.0f)]
                [field: DefaultValue(0.0f)]
                [field: DefaultValue(MyDouble)]
                [field: DefaultValue(MyFloat)]
                [property: DefaultValue(0.0)]
                [property: DefaultValue(1.24)]
                [property: DefaultValue(0.0f)]
                [property: DefaultValue(0.0f)]
                [property: DefaultValue(MyDouble)]
                [property: DefaultValue(MyFloat)]
                private void Test()
                {
                }
            }

            public class DefaultValueAttribute : Attribute
            {
                public DefaultValueAttribute(object value)
                {
                }
            }
            """;

        string result = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <summary>The backing field for <see cref="TestCommand"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::MyApp.DefaultValueAttribute(0D)]
                    [global::MyApp.DefaultValueAttribute(1.24D)]
                    [global::MyApp.DefaultValueAttribute(0F)]
                    [global::MyApp.DefaultValueAttribute(0F)]
                    [global::MyApp.DefaultValueAttribute(3.14D)]
                    [global::MyApp.DefaultValueAttribute(3.14F)]
                    private global::CommunityToolkit.Mvvm.Input.RelayCommand? testCommand;
                    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="Test"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    [global::MyApp.DefaultValueAttribute(0D)]
                    [global::MyApp.DefaultValueAttribute(1.24D)]
                    [global::MyApp.DefaultValueAttribute(0F)]
                    [global::MyApp.DefaultValueAttribute(0F)]
                    [global::MyApp.DefaultValueAttribute(3.14D)]
                    [global::MyApp.DefaultValueAttribute(3.14F)]
                    public global::CommunityToolkit.Mvvm.Input.IRelayCommand TestCommand => testCommand ??= new global::CommunityToolkit.Mvvm.Input.RelayCommand(new global::System.Action(Test));
                }
            }
            """;

        VerifyGenerateSources(source, new[] { new RelayCommandGenerator() }, ("MyApp.MyViewModel.Test.g.cs", result));
    }

    // See https://github.com/CommunityToolkit/dotnet/issues/632
    [TestMethod]
    public void RelayCommandMethodWithPartialDeclarations_TriggersCorrectly()
    {
        string source = """
            using CommunityToolkit.Mvvm.Input;

            #nullable enable

            namespace MyApp;
            
            partial class MyViewModel
            {
                [RelayCommand]
                private partial void Test1()
                {
                }

                private partial void Test1();

                private partial void Test2()
                {
                }

                [RelayCommand]
                private partial void Test2();
            }
            """;

        string result1 = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <summary>The backing field for <see cref="Test1Command"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    private global::CommunityToolkit.Mvvm.Input.RelayCommand? test1Command;
                    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="Test1"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    public global::CommunityToolkit.Mvvm.Input.IRelayCommand Test1Command => test1Command ??= new global::CommunityToolkit.Mvvm.Input.RelayCommand(new global::System.Action(Test1));
                }
            }
            """;

        string result2 = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <summary>The backing field for <see cref="Test2Command"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    private global::CommunityToolkit.Mvvm.Input.RelayCommand? test2Command;
                    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="Test2"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    public global::CommunityToolkit.Mvvm.Input.IRelayCommand Test2Command => test2Command ??= new global::CommunityToolkit.Mvvm.Input.RelayCommand(new global::System.Action(Test2));
                }
            }
            """;

        VerifyGenerateSources(source, new[] { new RelayCommandGenerator() }, ("MyApp.MyViewModel.Test1.g.cs", result1), ("MyApp.MyViewModel.Test2.g.cs", result2));
    }

    // See https://github.com/CommunityToolkit/dotnet/issues/632
    [TestMethod]
    public void RelayCommandMethodWithForwardedAttributesOverPartialDeclarations_MergesAttributes()
    {
        string source = """
            using CommunityToolkit.Mvvm.Input;

            #nullable enable

            namespace MyApp;
            
            partial class MyViewModel
            {
                [RelayCommand]
                [field: Value(0)]
                [property: Value(1)]
                private partial void Test1()
                {
                }

                [field: Value(2)]
                [property: Value(3)]
                private partial void Test1();

                [field: Value(0)]
                [property: Value(1)]
                private partial void Test2()
                {
                }

                [RelayCommand]
                [field: Value(2)]
                [property: Value(3)]
                private partial void Test2();
            }

            public class ValueAttribute : Attribute
            {
                public ValueAttribute(object value)
                {
                }
            }
            """;

        string result1 = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <summary>The backing field for <see cref="Test1Command"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::MyApp.ValueAttribute(2)]
                    [global::MyApp.ValueAttribute(0)]
                    private global::CommunityToolkit.Mvvm.Input.RelayCommand? test1Command;
                    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="Test1"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    [global::MyApp.ValueAttribute(3)]
                    [global::MyApp.ValueAttribute(1)]
                    public global::CommunityToolkit.Mvvm.Input.IRelayCommand Test1Command => test1Command ??= new global::CommunityToolkit.Mvvm.Input.RelayCommand(new global::System.Action(Test1));
                }
            }
            """;

        string result2 = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class MyViewModel
                {
                    /// <summary>The backing field for <see cref="Test2Command"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::MyApp.ValueAttribute(2)]
                    [global::MyApp.ValueAttribute(0)]
                    private global::CommunityToolkit.Mvvm.Input.RelayCommand? test2Command;
                    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="Test2"/>.</summary>
                    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.1.0.0")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    [global::MyApp.ValueAttribute(3)]
                    [global::MyApp.ValueAttribute(1)]
                    public global::CommunityToolkit.Mvvm.Input.IRelayCommand Test2Command => test2Command ??= new global::CommunityToolkit.Mvvm.Input.RelayCommand(new global::System.Action(Test2));
                }
            }
            """;

        VerifyGenerateSources(source, new[] { new RelayCommandGenerator() }, ("MyApp.MyViewModel.Test1.g.cs", result1), ("MyApp.MyViewModel.Test2.g.cs", result2));
    }

    [TestMethod]
    public void ObservablePropertyWithinGenericAndNestedTypes()
    {
        string source = """
            using System.ComponentModel;
            using CommunityToolkit.Mvvm.ComponentModel;

            #nullable enable

            namespace MyApp;
            
            partial class Foo
            {
                partial class MyViewModel<T> : ObservableObject
                {
                    [ObservableProperty]
                    private string? a;
                }
            }
            """;

        string result = """
            // <auto-generated/>
            #pragma warning disable
            #nullable enable
            namespace MyApp
            {
                partial class Foo
                {
                    partial class MyViewModel<T>
                    {
                        /// <inheritdoc cref="a"/>
                        [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                        public string? A
                        {
                            get => a;
                            set
                            {
                                if (!global::System.Collections.Generic.EqualityComparer<string?>.Default.Equals(a, value))
                                {
                                    OnAChanging(value);
                                    OnAChanging(default, value);
                                    OnPropertyChanging(global::CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.A);
                                    a = value;
                                    OnAChanged(value);
                                    OnAChanged(default, value);
                                    OnPropertyChanged(global::CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.A);
                                }
                            }
                        }

                        /// <summary>Executes the logic for when <see cref="A"/> is changing.</summary>
                        /// <param name="value">The new property value being set.</param>
                        /// <remarks>This method is invoked right before the value of <see cref="A"/> is changed.</remarks>
                        [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                        partial void OnAChanging(string? value);
                        /// <summary>Executes the logic for when <see cref="A"/> is changing.</summary>
                        /// <param name="oldValue">The previous property value that is being replaced.</param>
                        /// <param name="newValue">The new property value being set.</param>
                        /// <remarks>This method is invoked right before the value of <see cref="A"/> is changed.</remarks>
                        [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                        partial void OnAChanging(string? oldValue, string? newValue);
                        /// <summary>Executes the logic for when <see cref="A"/> just changed.</summary>
                        /// <param name="value">The new property value that was set.</param>
                        /// <remarks>This method is invoked right after the value of <see cref="A"/> is changed.</remarks>
                        [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                        partial void OnAChanged(string? value);
                        /// <summary>Executes the logic for when <see cref="A"/> just changed.</summary>
                        /// <param name="oldValue">The previous property value that was replaced.</param>
                        /// <param name="newValue">The new property value that was set.</param>
                        /// <remarks>This method is invoked right after the value of <see cref="A"/> is changed.</remarks>
                        [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
                        partial void OnAChanged(string? oldValue, string? newValue);
                    }
                }
            }
            """;

#if ROSLYN_4_3_1_OR_GREATER
        VerifyGenerateSources(source, new[] { new ObservablePropertyGenerator() }, ("MyApp.Foo+MyViewModel`1.g.cs", result));
#else
        VerifyGenerateSources(source, new[] { new ObservablePropertyGenerator() }, ("MyApp.Foo.MyViewModel_1.g.cs", result));
#endif
    }

    /// <summary>
    /// Generates the requested sources
    /// </summary>
    /// <param name="source">The input source to process.</param>
    /// <param name="generators">The generators to apply to the input syntax tree.</param>
    /// <param name="results">The source files to compare.</param>
    private static void VerifyGenerateSources(string source, IIncrementalGenerator[] generators, params (string Filename, string Text)[] results)
    {
        // Ensure CommunityToolkit.Mvvm and System.ComponentModel.DataAnnotations are loaded
        Type observableObjectType = typeof(ObservableObject);
        Type validationAttributeType = typeof(ValidationAttribute);

        // Get all assembly references for the loaded assemblies (easy way to pull in all necessary dependencies)
        IEnumerable<MetadataReference> references =
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
            where !assembly.IsDynamic
            let reference = MetadataReference.CreateFromFile(assembly.Location)
            select reference;

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10));

        // Create a syntax tree with the input source
        CSharpCompilation compilation = CSharpCompilation.Create(
            "original",
            new SyntaxTree[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators).WithUpdatedParseOptions((CSharpParseOptions)syntaxTree.Options);

        // Run all source generators on the input source code
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

        // Ensure that no diagnostics were generated
        CollectionAssert.AreEquivalent(Array.Empty<Diagnostic>(), diagnostics);

        foreach ((string filename, string text) in results)
        {
            SyntaxTree generatedTree = outputCompilation.SyntaxTrees.Single(tree => Path.GetFileName(tree.FilePath) == filename);

            Assert.AreEqual(text, generatedTree.ToString());
        }

        GC.KeepAlive(observableObjectType);
        GC.KeepAlive(validationAttributeType);
    }
}
