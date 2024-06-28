using Microsoft.CodeAnalysis;

namespace Omni.Generation.Extensions;

public static class ISymbolExtensions
{
  public static ITypeSymbol? GetSymbolType(this ISymbol symbol)
  {
    return symbol switch
    {
      IFieldSymbol fieldSymbol         => fieldSymbol.Type,
      IPropertySymbol propertySymbol   => propertySymbol.Type,
      IMethodSymbol methodSymbol       => methodSymbol.ReturnType,
      IParameterSymbol parameterSymbol => parameterSymbol.Type,
      ILocalSymbol localSymbol         => localSymbol.Type,
      _                                => null
    };
  }

  public static string GetLocalName(this ISymbol symbol)
  {
    string[] nameParts = symbol.Name.Split('.');
    return nameParts[nameParts.Length - 1];
  }
}