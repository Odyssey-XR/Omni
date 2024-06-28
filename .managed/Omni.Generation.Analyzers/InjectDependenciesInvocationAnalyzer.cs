using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Omni.Generation.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InjectDependenciesInvocationAnalyzer : DiagnosticAnalyzer
{
  private const           string            DiagnosticId  = nameof(InjectDependenciesInvocationAnalyzer);
  private const           string            Category      = "InitializationSafety";
  private static readonly LocalizableString Title         = "InjectDependencies invocation";
  private static readonly LocalizableString MessageFormat = "InjectDependencies should be invoked, and only once";
  private static readonly LocalizableString Description   = "InjectDependencies should be invoked, and only once.";

  private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
    DiagnosticId,
    Title,
    MessageFormat,
    Category,
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: Description,
    helpLinkUri: ""
  );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(EnsureMethodIsInvokedOnlyOnce, SyntaxKind.FieldDeclaration);
  }

  private void EnsureMethodIsInvokedOnlyOnce(SyntaxNodeAnalysisContext context)
  {
    FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)context.Node;
    IFieldSymbol?          fieldSymbol            = (IFieldSymbol)context.ContainingSymbol!;
    if (fieldSymbol is null || !HasInjectAttribute(fieldSymbol))
      return;

    SyntaxNode? classNode = fieldSymbol.ContainingType.DeclaringSyntaxReferences
      .FirstOrDefault()?
      .GetSyntax();

    if (classNode is null)
      return;

    IEnumerable<InvocationExpressionSyntax> invocationExpressions = classNode
      .DescendantNodes()
      .OfType<InvocationExpressionSyntax>();

    foreach (InvocationExpressionSyntax? expression in invocationExpressions)
    {
      if (ModelExtensions.GetSymbolInfo(context.SemanticModel, expression).Symbol is not IMethodSymbol methodSymbol)
        continue;

      if (methodSymbol.Name == "InjectDependencies")
        return;
    }

    Diagnostic diagnostic = Diagnostic.Create(Rule, fieldDeclarationSyntax.GetLocation());
    context.ReportDiagnostic(diagnostic);
  }

  private static bool HasInjectAttribute(ISymbol fieldSymbol)
  {
    return fieldSymbol.GetAttributes()
      .Any(ad => ad?.AttributeClass?.ToDisplayString() == "Omni.Attributes.InjectAttribute");
  }
}