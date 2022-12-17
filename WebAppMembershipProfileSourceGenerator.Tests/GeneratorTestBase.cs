using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace PageSourceGeneratorTests;

public abstract class GeneratorTestBase<TGenerator>
    where TGenerator : ISourceGenerator, new()
{
    private readonly List<string> _additionalSources;

    protected GeneratorTestBase(List<string> additionalSources)
    {
        _additionalSources = additionalSources;
    }

    protected void RunTestWithDriver(string source, Dictionary<string, string> generatedSources)
    {
        // Create the 'input' compilation that the generator will act on
        var sources = new List<string>(_additionalSources);
        sources.Add(source);
        Compilation inputCompilation = CreateCompilation(sources);

        // directly create an instance of the generator
        // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
        ISourceGenerator generator = new TGenerator();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = VisualBasicGeneratorDriver.Create(
            ImmutableArray.Create(generator));

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        if (!diagnostics.IsEmpty) {
            Assert.Fail(diagnostics.First().GetMessage());
        }

        var outputDiagnostics = outputCompilation.GetDiagnostics();
        Assert.True(outputDiagnostics.IsEmpty); // verify the compilation with the added source has no diagnostics

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        // The runResult contains the combined results of all generators passed to the driver
        Assert.True(runResult.Diagnostics.IsEmpty);

        foreach (var tree in runResult.GeneratedTrees)
        {
            var path = Path.GetFileName(tree.FilePath);
            if (generatedSources.TryGetValue(path, out var content))
            {
                Assert.Equal(content, tree.GetText().ToString());
                generatedSources.Remove(path);
            }
            else
            {
                Assert.Fail($"generated source was unexpected: {path}");
            }
        }
        if (generatedSources.Any())
        {
            Assert.Fail($"expected sources not found:\n{string.Join("\n", generatedSources.Select(kv => kv.Key))}");
        }

    }

    private static Compilation CreateCompilation(List<string> sources)
        => VisualBasicCompilation.Create("compilation",
            sources.Select(source => VisualBasicSyntaxTree.ParseText(source)).ToArray(),
            new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
            new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}