#nullable enable

using System.Collections.Generic;
using System.Linq;
using BetterBinding.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BetterBinding.CodeGen;

internal class SyntaxCollector : ISyntaxReceiver
{
    public List<TypeDeclarationSyntax> WorkItems { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax type && IsCandidateType(type))
        {
            WorkItems.Add(type);
        }
    }

    private static bool IsCandidateType(TypeDeclarationSyntax? syntax)
    {
        if (syntax is null)
        {
            return false;
        }
        
        if (!syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        return syntax.GetBindableProperties().Any();
    }

}
