using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using BusinessCentral.LinterCop.Helpers;
using Microsoft.Dynamics.Nav.Analyzers.Common.AppSourceCopConfiguration;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.CodeDom;

namespace BusinessCentral.LinterCop.Design
{
    [DiagnosticAnalyzer]
    public class Rule0099TODOsMustNotBePresent : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create<DiagnosticDescriptor>(DiagnosticDescriptors.Rule0099TODOsMustNotBePresent);

        public override void Initialize(AnalysisContext context)
            => context.RegisterSymbolAction(new Action<SymbolAnalysisContext>(this.CheckForTODOs), SymbolKind.Codeunit, SymbolKind.Enum, SymbolKind.Interface, SymbolKind.PermissionSet, SymbolKind.Query, SymbolKind.Table, SymbolKind.Page, SymbolKind.Report, SymbolKind.XmlPort);

        private void CheckForTODOs(SymbolAnalysisContext ctx)
        {
            var manifest = AppSourceCopConfigurationProvider.GetManifest(ctx.Compilation);
            if (manifest.Runtime < RuntimeVersion.Spring2021 && (ctx.Symbol.Kind == SymbolKind.Enum || ctx.Symbol.Kind == SymbolKind.Interface))
                return;

            foreach (var trivia in ctx.Symbol.DeclaringSyntaxReference.GetSyntax().DescendantTrivia().Where(t => IsCommentTrivia(t)))
            {
                if (trivia.ToString().Contains("TODO"))
                {
                    if (!trivia.ContainsDiagnostics)
                    {
                        var text = trivia.ToString().Substring(trivia.ToString().IndexOf("TODO"));
                        text = text.Split('\n', '\r')[0];
                        ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0099TODOsMustNotBePresent, trivia.GetLocation(),new Object[] { text }));
                    }
                }
            }
        }
        private bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            var commentKinds = new List<SyntaxKind> { SyntaxKind.CommentTrivia, SyntaxKind.DocumentationCommentExteriorTrivia, SyntaxKind.EndOfDocumentationCommentToken, SyntaxKind.LineCommentTrivia, SyntaxKind.MultiLineDocumentationCommentTrivia, SyntaxKind.SingleLineDocumentationCommentTrivia };

            return commentKinds.Contains(trivia.Kind);
        }
    }
}
