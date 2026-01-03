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
    public Dictionary<string, (List<string> Usings, string? Namespace)> CollectionsToCreate { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TypeDeclarationSyntax type || !IsCandidateType(type))
        {
            return;
        }

        List<UsingDirectiveSyntax> namespaces = [];
        var current = syntaxNode;
        while (current != null)
        {
            switch (current)
            {
                case NamespaceDeclarationSyntax n:
                    namespaces.AddRange(n.Usings);
                    break;
                case FileScopedNamespaceDeclarationSyntax fileNamespace:
                    namespaces.AddRange(fileNamespace.Usings);
                    break;
                case CompilationUnitSyntax cu:
                    namespaces.AddRange(cu.Usings);
                    break;
            }
            
            current = current.Parent;
        }

        var hasBindableProperties = false;

        var fullNamespace = type.GetFullNamespace();
        foreach (var member in type.Members)
        {
            if (member is PropertyDeclarationSyntax property)
            {
                if (property.Type.ToString().Contains("Property<"))
                {
                    hasBindableProperties = true;                   
                }

                if (property.Type.ToString().Contains("CollectionViewModel<"))
                {
                    hasBindableProperties = true;
                    CollectionsToCreate[property.Type.ToString()] =
                        (namespaces.Select(ns => ns.Name.ToString()).ToList(),
                        fullNamespace);
                }
            }
        }

        if (hasBindableProperties)
        {
            WorkItems.Add(type);
        }
    }

    private static bool IsCandidateType(TypeDeclarationSyntax? syntax)
    {
        return syntax is not null && syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

}
