using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace burtonrodman
{
    public class PartialClassSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassBlockSyntax> AllPartialClassBlocks { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassBlockSyntax cbs &&
                cbs.ClassStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
            )
            {
                AllPartialClassBlocks.Add(cbs);
            }
        }
    }
}
