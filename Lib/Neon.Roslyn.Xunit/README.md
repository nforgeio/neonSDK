# Neon.Roslyn.Xunit

This package provides some utilities to test Roslyn analyzers, code fix providers, and source generators using FluentAssertions.

## Install
```sh
dotnet add package Neon.Roslyn.Xunit
```

## Testing an analyzer

### Verify generated source code
```csharp
var inputSource = @"
namespace MyNamespace
{
    public partial class MyClass
    {
        public void MyMethod()
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";

var expectedSource = @"
namespace MyNamespace
{
    public partial class MyClass
    {
        public void MyGeneratedMethod()
        {
            Console.WriteLine(""Hello, Roslyn!"");
        }
    }
}";

var testCompilation = new TestCompilationBuilder()
    .AddSourceGenerator<MySourceGenerator>()
    .AddSource(source)
    .Build();

// verify that the expected source code was generated
testCompilation.Should().ContainSource(expectedSource);

// verify the number of generated source files
testCompilation.Sources.Should().HaveCount(1);
```

### Verify diagnostics
```csharp
var testCompilation = new TestCompilationBuilder()
    .AddSourceGenerator<MySourceGenerator>()
    .AddSource(mySource)
    .Build();

// verify the number of diagnostics
testCompilation.Diagnostics.Should().HaveCount(2);

// verify the specific diagnostics using the diagnostic code
testCompilation.Should().HaveDiagnostics(["D1001", "D1002"]);

// verify the specific diagnostics using the diagnostic descriptor
testCompilation.Should().HaveDiagnostic(
    diagnostic: Diagnostic.Create(
        descriptor:  MyDiagnosticDescriptor,
        location:    Location.None,
        messageArgs: ["foo", 123]));

```

### Adding assemblies to the compilation
```csharp
var testCompilation = new TestCompilationBuilder()
    .AddSourceGenerator<MySourceGenerator>()
    .AddAssembly(typeof(MyType).Assembly)
    .Build();
```

### Adding MSBuild options to the compilation
```csharp
var testCompilation = new TestCompilationBuilder()
    .AddSourceGenerator<MySourceGenerator>()
    .AddOption("build_property.TargetFramework", "net8.0")
    .Build();
```