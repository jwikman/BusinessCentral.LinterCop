using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BusinessCentral.LinterCop.Design
{
    [DiagnosticAnalyzer]
    public class BuiltInMethodImplementThroughCodeunit : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create<DiagnosticDescriptor>(
            DiagnosticDescriptors.Rule0021ConfirmImplementConfirmManagement,
            DiagnosticDescriptors.Rule0022GlobalLanguageImplementTranslationHelper,
            DiagnosticDescriptors.Rule0000ErrorInRule);

        public override void Initialize(AnalysisContext context) => context.RegisterOperationAction(new Action<OperationAnalysisContext>(this.CheckBuiltInMethod), OperationKind.InvocationExpression);

        private void CheckBuiltInMethod(OperationAnalysisContext ctx)
        {
            if (ctx.ContainingSymbol.GetContainingObjectTypeSymbol().IsObsoletePending || ctx.ContainingSymbol.GetContainingObjectTypeSymbol().IsObsoleteRemoved) return;
            if (ctx.ContainingSymbol.IsObsoletePending || ctx.ContainingSymbol.IsObsoleteRemoved) return;

            IInvocationExpression operation = (IInvocationExpression)ctx.Operation;
            if (operation.TargetMethod.MethodKind != MethodKind.BuiltInMethod) return;

            switch (operation.TargetMethod.Name.ToUpper())
            {
                case "CONFIRM":
                    try
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0021ConfirmImplementConfirmManagement, ctx.Operation.Syntax.GetLocation()));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0000ErrorInRule, ctx.Operation.Syntax.GetLocation(), new Object[] { "Rule0021", "ArgumentOutOfRangeException", "at Line 29" }));
                    }
                    break;
                case "GLOBALLANGUAGE":
                    try
                    {
                        if (operation.Arguments.Length != 0) { ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0022GlobalLanguageImplementTranslationHelper, ctx.Operation.Syntax.GetLocation())); }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Rule0000ErrorInRule, ctx.Operation.Syntax.GetLocation(), new Object[] { "Rule0022", "ArgumentOutOfRangeException", "at Line 39" }));
                    }
                    break;
            }
        }
    }
}