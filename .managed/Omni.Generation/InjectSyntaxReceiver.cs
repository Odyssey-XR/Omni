using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Omni.Generation;

public class InjectSyntaxReceiver : ISyntaxContextReceiver
{
  public List<IPropertySymbol> CandidateProperties { get; } = new();

  public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
  {
    if (!(context.Node is PropertyDeclarationSyntax propertyDeclaration) || propertyDeclaration.AttributeLists.Count < 1)
      return;

    IPropertySymbol? propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
    if (propertySymbol is null)
      return;

    foreach (AttributeData? attributeData in propertySymbol.GetAttributes())
    {
      if (attributeData == null || attributeData.AttributeClass == null)
        continue;

      string name = attributeData.AttributeClass.ToDisplayString();
      if (name == "Omni.Attributes.InjectAttribute")
        CandidateProperties.Add(propertySymbol);
    }
  }
}