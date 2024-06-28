using Microsoft.CodeAnalysis;

namespace Omni.Generation.Extensions;

public static class ITypeSymbolExtensions
{
  public static bool InheritsFrom(this ITypeSymbol typeSymbol, string baseType)
  {
    if (typeSymbol is null)
      return false;

    if (typeSymbol.BaseType is null)
      return typeSymbol.ToDisplayString() == baseType;
    return typeSymbol.ToDisplayString() == baseType || typeSymbol.BaseType.InheritsFrom(baseType);
  }
}