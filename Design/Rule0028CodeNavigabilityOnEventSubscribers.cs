using Microsoft.Dynamics.Nav.Analyzers.Common.AppSourceCopConfiguration;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace BusinessCentral.LinterCop.Design
{
    [DiagnosticAnalyzer]
    public class Rule0028CodeNavigabilityOnEventSubscribers : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create<DiagnosticDescriptor>(DiagnosticDescriptors.Rule0028CodeNavigabilityOnEventSubscribers);

        public override void Initialize(AnalysisContext context) => context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(this.CodeNavigabilityOnEventSubscribers));

        private void CodeNavigabilityOnEventSubscribers(CodeBlockAnalysisContext context)
        {
            // Support for using Identifiers instead of Literals in event subscribers is supported from runtime versions: '11.0' or greater.
            var manifest = AppSourceCopConfigurationProvider.GetManifest(context.SemanticModel.Compilation);
            if (manifest.Runtime < RuntimeVersion.Spring2023) return;

            if (context.OwningSymbol.GetContainingObjectTypeSymbol().IsObsoletePending || context.OwningSymbol.GetContainingObjectTypeSymbol().IsObsoleteRemoved) return;
            if (context.OwningSymbol.IsObsoletePending || context.OwningSymbol.IsObsoleteRemoved) return;

            if (!context.CodeBlock.IsKind(SyntaxKind.MethodDeclaration)) return;
            var SyntaxList = ((MethodDeclarationSyntax)context.CodeBlock).Attributes.Where(value => value.GetIdentifierOrLiteralValue().ToUpper() == "EVENTSUBSCRIBER");

            foreach (var Syntax in SyntaxList)
            {
                if (Syntax.ArgumentList.Arguments[2].IsKind(SyntaxKind.LiteralAttributeArgument))
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0028CodeNavigabilityOnEventSubscribers, context.OwningSymbol.GetLocation()));
            }
        }
    }
}