using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Omni.Generation.Tests
{
  public class InjectAttributeGeneratorTests
  {
    [Fact]
    public void Test1()
    {
      string source = $@"
namespace test
{{
  public interface ITest {{ }}

  public partial class MyClass
  {{
    [Inject] private ITest testProperty {{ get; set; }}
  }}
}}
";
      InjectGenerator generator = new InjectGenerator();
      var compilation = CSharpCompilation
        .Create("CSharpCodeGen.GenerateAssembly")
        .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source))
        .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
        .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

      var driver = CSharpGeneratorDriver
        .Create(generator)
        .RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> _);

      var result = driver.GetRunResult();
    }
  }
}