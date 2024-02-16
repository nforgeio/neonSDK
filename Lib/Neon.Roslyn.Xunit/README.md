# Neon.Roslyn.Xunit

This package provides some utilities to test Roslyn analyzers, code fix providers, and source generators.

## Install
```sh
dotnet add package Neon.Roslyn.Xunit
```

## Usage

Testing an analyzer
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

testCompilation.Should().ContainSource(expectedSource);
testCompilation.Should().HaveCount(1);
```

Adding assemblies to the compilation
```csharp
var testCompilation = new TestCompilationBuilder()
    .AddSourceGenerator<MySourceGenerator>()
    .AddAssembly(typeof(MyType).Assembly)
    .Build();
```

Adding MSBuild options to the compilation
```csharp
var testCompilation = new TestCompilationBuilder()
    .AddSourceGenerator<MySourceGenerator>()
    .AddOption("build_property.TargetFramework", "net8.0")
    .Build();
```