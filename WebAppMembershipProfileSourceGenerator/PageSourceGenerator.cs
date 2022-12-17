using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace burtonrodman.WebAppMembershipProfileSourceGenerator
{
    [Generator(LanguageNames.VisualBasic)]
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
                var generated = new Dictionary<string, string>();
                foreach (var page in receiver.AllPartialClassBlocks)
                {
                    try
                    {
                        var model = context.Compilation.GetSemanticModel(page.SyntaxTree);

                        var symbol = model.GetDeclaredSymbol(page);
                        var ns = symbol.ContainingNamespace.ToString();
                        var className = page.ClassStatement.Identifier.Text;
                        var fullClassName = $"{ns}.{className}";
                        if (symbol.BaseType?.ToString() == "System.Web.UI.Page" &&
                            !generated.ContainsKey(fullClassName)
                        ) {
                            context.AddSource($"{fullClassName}.g.vb",
                                $"""
                                Partial Class {className}
                                    Property Profile As ProfileCommon
                                End Class
                                """);
                            generated.Add(fullClassName, fullClassName);
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
