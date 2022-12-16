using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace burtonrodman.WebAppMembershipProfileSourceGenerator
{
    [Generator]
    public class PageSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new PartialClassSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // The SyntaxReceiver first get a chance to filter the nodes that we care about
            // now we need to loop over the collected nodes and generate the source files.

            if (context.SyntaxReceiver is PartialClassSyntaxReceiver receiver)
            {
                foreach (var page in receiver.AllPartialClassBlocks)
                {
                    try
                    {
                        var model = context.Compilation.GetSemanticModel(page.SyntaxTree);

                        var symbol = model.GetDeclaredSymbol(page);
                        if (symbol.BaseType?.ToString() == "System.Web.UI.Page")
                        {
                            context.AddSource($"{page.ClassStatement.Identifier.Text}.g.vb",
                                $"""
                                Partial Class {page.ClassStatement.Identifier.Text}
                                    Property Profile As ProfileCommon
                                End Class
                                """);
                        }
                    }
                    catch (Exception ex)
                    {
                        var descriptor = new DiagnosticDescriptor(
                            "MPG001", "An error occurred generating Profile.",
                            "{0}", "Source Generation", DiagnosticSeverity.Error, true);

                        context.ReportDiagnostic(Diagnostic.Create(descriptor, page.GetLocation(), ex.ToString()));
                    }
                }
            }
        }
    }
}
