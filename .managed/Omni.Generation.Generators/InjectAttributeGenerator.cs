using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Omni.Generation.SyntaxReceivers;
using Omni.Generation.Extensions;

namespace Omni.Generation.Generators
{
  [Generator]
  public class InjectAttributeGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
      context.RegisterForSyntaxNotifications(() => new InjectAttributeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
      if (context.SyntaxContextReceiver is not InjectAttributeSyntaxReceiver receiver)
        return;

      foreach (ISymbol symbol in receiver.CandidateSymbols)
      {
        if (symbol is not IMethodSymbol methodSymbol)
          continue;

        string classSource = GenerateClassWithInjectFields(symbol.ContainingType, methodSymbol);
        context.AddSource($"{symbol.ContainingType.ToDisplayString()}_DependencyResolver.g.cs",
          SourceText.From(classSource, Encoding.UTF8));
      }
    }

    private string GenerateClassWithInjectFields(
      ISymbol parentSymbol,
      IMethodSymbol methodSymbol
    )
    {
      // Create property getters
      StringBuilder usingDecl     = new();
      StringBuilder propertyDecl  = new();
      StringBuilder parameterDecl = new();
      for (int i = 0; i < methodSymbol.Parameters.Length; i++)
      {
        IParameterSymbol parameter    = methodSymbol.Parameters[i];
        string parameterAssembly      = parameter.GetSymbolType()?.ContainingNamespace.ToDisplayString() ?? "";
        string parameterType          = (parameter.GetSymbolType()?.GetLocalName() ?? "").Replace("?", "");
        string parameterName          = parameter.GetLocalName();
        string parameterAccessibility = parameter.GetAttributes().FirstOrDefault()?.AttributeClass?.ToDisplayString() switch
        {
          "Omni.Attributes.PublicAttribute"    => "public",
          "Omni.Attributes.ProtectedAttribute" => "protected",
          "Omni.Attributes.PrivateAttribute"   => "private",
          _                                    => "private"
        };

        usingDecl.AppendLine($"using {parameterAssembly};");

        propertyDecl.AppendLine(
          $"    {parameterAccessibility} {parameterType}? {parameterName} => ContainerProvider.GetInstanceOf<{parameterType}>();");

        parameterDecl.AppendLine(i + 1 == methodSymbol.Parameters.Length
          ? $"      {parameterType} {parameterName}"
          : $"      {parameterType} {parameterName},");
      }

      // Create partial type and inject method 
      string namespaceName       = parentSymbol.ContainingNamespace.ToDisplayString();
      string parentAccessibility = parentSymbol.DeclaredAccessibility.ToString().ToLower();
      string parentName          = parentSymbol.Name;
      string parentType          = ((ITypeSymbol)parentSymbol).TypeKind.ToString().ToLower();
      string methodAccessibility = methodSymbol.DeclaredAccessibility.ToString().ToLower();
      string methodName          = methodSymbol.GetLocalName().Replace("(", "").Replace(")", "");
      return $$"""
               #nullable enable
               using Omni.Providers;
               {{usingDecl}}
               namespace {{namespaceName}}
               {
                 {{parentAccessibility}} partial {{parentType}} {{parentName}}
                 {
               {{propertyDecl}}
                   {{methodAccessibility}} partial void {{methodName}}(
               {{parameterDecl}}    ) {}
                 }
               }
               """;
    }
  }
}