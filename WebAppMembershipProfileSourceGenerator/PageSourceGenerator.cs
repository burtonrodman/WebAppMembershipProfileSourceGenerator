using System;
using System.Collections.Generic;
using System.Linq;
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

            var rootNamespace = context.GetMSBuildProperty("RootNamespace");
            if (context.SyntaxReceiver is PartialClassSyntaxReceiver receiver)
            {
                var generated = new Dictionary<string, string>();
                foreach (var page in receiver.AllPartialClassBlocks)
                {
                    try
                    {
                        var model = context.Compilation.GetSemanticModel(page.SyntaxTree);

                        var symbol = model.GetDeclaredSymbol(page);
                        var ns = symbol.GetContainingNamespace();
                        var thisNs = ns.GetNamespaceWithoutRoot(rootNamespace);
                        var className = page.ClassStatement.Identifier.Text;
                        var fullClassName = $"{ns}.{className}";
                        if (symbol.BaseType?.ToString() == "System.Web.UI.Page" &&
                            !generated.ContainsKey(fullClassName)
                        )
                        {
                            if (ns == rootNamespace)
                            {
                                context.AddSource($"{fullClassName}.g.vb",
                                    $"""
                                    ' ns = {ns}
                                    ' thisNs = {thisNs}
                                    ' fullClassName = {fullClassName}
                                    ' rootNamespace = {rootNamespace}
                                    Partial Class {className}
                                        Property Profile As ProfileCommon
                                    End Class
                                    """);
                            }
                            else
                            {
                                context.AddSource($"{fullClassName}.g.vb",
                                    $"""
                                    ' ns = {ns}
                                    ' thisNs = {thisNs}
                                    ' fullClassName = {fullClassName}
                                    ' rootNamespace = {rootNamespace}
                                    Namespace {thisNs}
                                        Partial Class {className}
                                            Property Profile As ProfileCommon
                                        End Class
                                    End Namespace
                                    """);
                            }
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

public static class SymbolExtensions
{
    public static string GetContainingNamespace(this INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace.ToString();
        var parts = ns.Split(".".ToCharArray()).ToList();
        if (parts.First() == "Global") parts.RemoveAt(0);
        return string.Join(".", parts);
    }

    public static string GetNamespaceWithoutRoot(this string ns, string rootNamespace)
    {
        var parts = ns.Split(".".ToCharArray()).ToList();
        if (parts.FirstOrDefault() == rootNamespace) parts.RemoveAt(0);
        return string.Join(".", parts);
    }
}
