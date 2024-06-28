using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Omni.Generation.SyntaxReceivers;
using Omni.Generation.Extensions;

namespace Omni.Generation.Generators
{
  [Generator]
  public class ComponentAttributeGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
      context.RegisterForSyntaxNotifications(() => new ComponentAttributeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
      if (context.SyntaxContextReceiver is not ComponentAttributeSyntaxReceiver receiver)
        return;

      foreach (IGrouping<ISymbol, ISymbol> group in receiver.CandidateSymbols
                 .GroupBy<ISymbol, ISymbol>(_ => _.ContainingType, SymbolEqualityComparer.Default))
      {
        // if (!((ITypeSymbol)group.Key).InheritsFrom("UnityEngine.MonoBehaviour"))
        //   continue;

        string classSource = GenerateClassWithInjectFields(group.Key, group);
        context.AddSource($"{group.Key.Name}_ComponentResolver.g.cs", SourceText.From(classSource, Encoding.UTF8));
      }
    }

    private string GenerateClassWithInjectFields(
      ISymbol parentSymbol,
      IEnumerable<ISymbol> propertySymbols
    )
    {
      string namespaceName       = parentSymbol.ContainingNamespace.ToDisplayString();
      string parentAccessibility = parentSymbol.DeclaredAccessibility.ToString().ToLower();
      string parentName          = parentSymbol.Name;
      string parentType          = ((ITypeSymbol)parentSymbol).TypeKind.ToString().ToLower();

      StringBuilder stringBuilder = new($$"""
                                          #nullable enable
                                          using UnityEngine;

                                          namespace {{namespaceName}}
                                          {
                                          """);

      foreach (ISymbol propertySymbol in propertySymbols)
      {
        string propertyType = propertySymbol.GetSymbolType()?.ToDisplayString() ?? "";

        stringBuilder.AppendLine($$"""
                                     [RequireComponent(typeof({{propertyType}}))]
                                   """);
      }

      stringBuilder.AppendLine($$"""
                                 {{parentAccessibility}} partial class {{parentName}}
                                 {
                                    protected void ResolveComponents()
                                    {
                               """);


      foreach (ISymbol propertySymbol in propertySymbols)
      {
        string propertyType = propertySymbol.GetSymbolType()?.ToDisplayString() ?? "";
        string propertyName = propertySymbol.Name;

        stringBuilder.AppendLine($$"""
                                       {{propertyName}} = this.GetComponent<{{propertyType.Replace("?", "h")}}>();
                                   """);
      }

      stringBuilder.AppendLine("""
                                   }
                                 }
                               }
                               """);

      return stringBuilder.ToString();
    }
  }
}