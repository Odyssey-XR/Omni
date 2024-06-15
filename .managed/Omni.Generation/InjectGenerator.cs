using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Omni.Generation
{
  [Generator]
  public class InjectGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
      context.RegisterForSyntaxNotifications(() => new InjectSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
      if (!(context.SyntaxContextReceiver is InjectSyntaxReceiver receiver))
        return;

      foreach (IGrouping<INamedTypeSymbol, IPropertySymbol> group in receiver.CandidateProperties
                 .GroupBy<IPropertySymbol, INamedTypeSymbol>(_ => _.ContainingType, SymbolEqualityComparer.Default))
      {
        string classSource = GenerateClassWithInjectFields(group.Key, group);
        context.AddSource($"{group.Key.Name}_Injection.g.cs", SourceText.From(classSource, Encoding.UTF8));
      }
    }

    private string GenerateClassWithInjectFields(
      ISymbol parentSymbol,
      IEnumerable<IPropertySymbol> propertySymbols
    )
    {
      string namespaceName = parentSymbol.ContainingNamespace.ToDisplayString();
      string parentAccessibility = parentSymbol.DeclaredAccessibility.ToString().ToLower();
      string parentName = parentSymbol.Name;
      string parentType = ((ITypeSymbol)parentSymbol).TypeKind.ToString().ToLower();

      StringBuilder stringBuilder = new StringBuilder($@"
using UnityEngine;
namespace {namespaceName}
{{
    public partial class {parentName}
    {{
");

//       foreach (IPropertySymbol propertySymbol in propertySymbols)
//       {
//         string propertyAccessibility = propertySymbol.DeclaredAccessibility.ToString().ToLower();
//         string propertyType = propertySymbol.Type.ToDisplayString();
//         string propertyName = propertySymbol.Name;
//
//         stringBuilder.AppendLine($@"
//         {propertyAccessibility} {propertyType} {propertyName}
//         {{
//             get => default;
//         }}
// ");
//       }

      stringBuilder.AppendLine(@"
      public void Awake()
      {
        Debug.Log(""Hello World"");
      }
    }
}");

      return stringBuilder.ToString();
    }
  }
}