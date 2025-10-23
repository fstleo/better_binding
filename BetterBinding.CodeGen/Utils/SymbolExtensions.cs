#nullable enable

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BetterBinding.CodeGen.Utils;

public static class SymbolExtensions
{
    public static IEnumerable<PropertyDeclarationSyntax> GetBindableProperties(this TypeDeclarationSyntax type)
    {
        foreach (var member in type.Members)
        {
            if (member is PropertyDeclarationSyntax property && IsBindableProperty(member))
            {
                yield return property;
            }
        }
    }

    private static bool IsBindableProperty(this MemberDeclarationSyntax member)
    {
        return member is PropertyDeclarationSyntax property && property.Type.ToString().Contains("Property<");
                                                                //|| IsBindableContract(property)); //TODO: add contracts check
    }

    public static string? GetFullNamespace(this TypeDeclarationSyntax type)
    {
        string? namespaceName = null;
        SyntaxNode? parent = type.Parent;

        while (parent != null)
        {
            switch (parent)
            {
                case NamespaceDeclarationSyntax namespaceDecl:
                    namespaceName = namespaceDecl.Name + (namespaceName == null ? "" : "." + namespaceName);
                    break;
                case FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDecl:
                    namespaceName = fileScopedNamespaceDecl.Name + (namespaceName == null ? "" : "." + namespaceName);
                    break;
            }

            parent = parent.Parent;
        }

        return namespaceName;
    }

    public static IEnumerable<INamedTypeSymbol> GetAllContracts(this INamedTypeSymbol symbol)
    {
        yield return symbol;
        foreach (var i in symbol.Interfaces)
        {
            yield return i;
        }
        
        var t = symbol.BaseType;
        while (t != null && t.Name != "Object")
        {
            foreach (var i in t.Interfaces)
            {
                yield return i;
            }
            
            yield return t;
            t = t.BaseType;
        }
    }
}