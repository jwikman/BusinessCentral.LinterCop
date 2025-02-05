﻿using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BusinessCentral.LinterCop.Design
{
    [DiagnosticAnalyzer]
    public class Rule0003DoNotUseObjectIDsInVariablesOrProperties : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create<DiagnosticDescriptor>(DiagnosticDescriptors.Rule0003DoNotUseObjectIDsInVariablesOrProperties, DiagnosticDescriptors.Rule0005VariableCasingShouldNotDifferFromDeclaration);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(this.CheckForObjectIDsInVariablesOrProperties), new SyntaxKind[] {
                SyntaxKind.ObjectReference,
                SyntaxKind.PermissionValue
            });

        }
        private void CheckForObjectIDsInVariablesOrProperties(SyntaxNodeAnalysisContext ctx)
        {
            if (ctx.ContainingSymbol.IsObsoletePending || ctx.ContainingSymbol.IsObsoleteRemoved) return;
            if (ctx.ContainingSymbol.GetContainingObjectTypeSymbol().IsObsoletePending || ctx.ContainingSymbol.GetContainingObjectTypeSymbol().IsObsoleteRemoved) return;

            var correctName = "";

            if (ctx.ContainingSymbol.Kind == SymbolKind.LocalVariable || ctx.ContainingSymbol.Kind == SymbolKind.GlobalVariable)
            {
                IVariableSymbol variable = (IVariableSymbol)ctx.ContainingSymbol;
                if (variable.Type.NavTypeKind == NavTypeKind.DotNet) return;

                if (variable.Type.NavTypeKind == NavTypeKind.Array)
                    correctName = ((IArrayTypeSymbol)variable.Type).ElementType.Name.ToString();
                else
                    correctName = variable.Type.Name;

                if (ctx.Node.GetLastToken().ToString().Trim('"').ToUpper() != correctName.ToUpper())
                    ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0003DoNotUseObjectIDsInVariablesOrProperties, ctx.Node.GetLocation(), new object[] { ctx.Node.ToString().Trim('"'), correctName }));

                if (ctx.Node.GetLastToken().ToString().Trim('"') != correctName)
                    ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0005VariableCasingShouldNotDifferFromDeclaration, ctx.Node.GetLocation(), new object[] { correctName, "" }));
            }
            if (ctx.ContainingSymbol.Kind == SymbolKind.Property)
            {
                IPropertySymbol property = (IPropertySymbol)ctx.ContainingSymbol;

                if (ctx.Node.Kind == SyntaxKind.PermissionValue)
                {
                    var nodes = ctx.Node.ChildNodesAndTokens().GetEnumerator();

                    while (nodes.MoveNext())
                    {
                        if (nodes.Current.IsNode)
                        {
                            var subnodes = nodes.Current.ChildNodesAndTokens().GetEnumerator();

                            while (subnodes.MoveNext())
                            {
                                if (subnodes.Current.Kind == SyntaxKind.ObjectId)
                                    ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0003DoNotUseObjectIDsInVariablesOrProperties, nodes.Current.GetLocation(), new object[] { "", "the object name" }));
                            };
                        }

                    };
                }

                if (property.PropertyKind != PropertyKind.Permissions && property.PropertyKind != PropertyKind.AccessByPermission)
                {
                    if (ctx.Node.GetLastToken().ToString().Trim('"').ToUpper() != property.ValueText.ToUpper())
                        ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0003DoNotUseObjectIDsInVariablesOrProperties, ctx.Node.GetLocation(), new object[] { ctx.Node.ToString().Trim('"'), property.ValueText }));

                    if (ctx.Node.GetLastToken().ToString().Trim('"') != property.ValueText)
                        ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0005VariableCasingShouldNotDifferFromDeclaration, ctx.Node.GetLocation(), new object[] { property.ValueText, "" }));
                }
            }

            if (ctx.ContainingSymbol.Kind == SymbolKind.Method)
            {
                IMethodSymbol method = (IMethodSymbol)ctx.ContainingSymbol;

                foreach (IParameterSymbol parameter in method.Parameters)
                {
                    if (parameter.ParameterType.NavTypeKind == NavTypeKind.DotNet)
                    {
                        continue;
                    }

                    if (ctx.Node.GetLocation().SourceSpan.End == parameter.DeclaringSyntaxReference.GetSyntax(CancellationToken.None).Span.End)
                    {
                        if (parameter.ParameterType.NavTypeKind == NavTypeKind.Array)
                            correctName = ((IArrayTypeSymbol)(parameter.ParameterType)).ElementType.Name.ToString();
                        else
                            correctName = parameter.ParameterType.Name;

                        if (ctx.Node.GetLastToken().ToString().Trim('"').ToUpper() != correctName.ToUpper())
                            ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0003DoNotUseObjectIDsInVariablesOrProperties, ctx.Node.GetLocation(), new object[] { ctx.Node.ToString().Trim('"'), correctName }));

                        if (ctx.Node.GetLastToken().ToString().Trim('"') != correctName)
                            ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0005VariableCasingShouldNotDifferFromDeclaration, ctx.Node.GetLocation(), new object[] { correctName, "" }));
                    }
                }
                try
                {
                    IReturnValueSymbol returnValue = method.ReturnValueSymbol;
                    if (returnValue.ReturnType.NavTypeKind == NavTypeKind.DotNet)
                    {
                        return;
                    }

                    if (ctx.Node.GetLocation().SourceSpan.End == returnValue.DeclaringSyntaxReference.GetSyntax(CancellationToken.None).Span.End)
                    {
                        correctName = returnValue.ReturnType.Name;

                        if (ctx.Node.ToString().Trim('"').ToUpper() != correctName.ToUpper())
                            ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0003DoNotUseObjectIDsInVariablesOrProperties, ctx.Node.GetLocation(), new object[] { ctx.Node.ToString().Trim('"'), correctName }));

                        if (ctx.Node.ToString().Trim('"') != correctName)
                            ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0005VariableCasingShouldNotDifferFromDeclaration, ctx.Node.GetLocation(), new object[] { correctName, "" }));
                    }
                }
                catch (System.NullReferenceException)
                { }
            }
        }
    }
}
