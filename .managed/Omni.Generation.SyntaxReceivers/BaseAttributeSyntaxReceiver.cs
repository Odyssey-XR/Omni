using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Omni.Generation.SyntaxReceivers;

public abstract class BaseAttributeSyntaxReceiver : ISyntaxContextReceiver
{
  private string        _attributeName;
  public  List<ISymbol> CandidateSymbols { get; } = [];

  public BaseAttributeSyntaxReceiver(string attributeName)
  {
    _attributeName = attributeName;
  }
  
  public virtual void OnVisitSyntaxNode(GeneratorSyntaxContext context)
  {
    if (context.Node is not MemberDeclarationSyntax declarationNode || declarationNode.AttributeLists.Count < 1)
      return;

    ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(declarationNode);
    if (symbol is null)
      return;

    foreach (AttributeData? attributeData in symbol.GetAttributes())
    {
      if (attributeData?.AttributeClass is null)
        continue;

      string name = attributeData.AttributeClass.ToDisplayString();
      if (name == _attributeName)
        CandidateSymbols.Add(symbol);
    }
  }
}