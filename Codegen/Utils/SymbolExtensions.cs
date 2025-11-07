#nullable enable

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BetterBinding.CodeGen.Utils;

public static class SymbolExtensions
{
    private static readonly string[] BindableInterfaces = 
    [
        "IObservable", 
        "IViewModel"
    ];

    public static IEnumerable<PropertyDeclarationSyntax> GetBindableProperties(this TypeDeclarationSyntax type, 
        SemanticModel semanticModel)
    {
        foreach (var member in type.Members)
        {
            if (member is PropertyDeclarationSyntax property && IsBindableProperty(member, semanticModel))
            {
                yield return property;
            }
        }
    }

    private static bool IsBindableProperty(this MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (member is not PropertyDeclarationSyntax property)
        {
            return false;
        }
        
        var propertyType = semanticModel.GetTypeInfo(property.Type);
        if (propertyType.Type is null)
        {
            return false;
        }
            
        foreach (var implementedInterface in propertyType.Type.AllInterfaces)
        {
            foreach (var bindableInterface in BindableInterfaces)
            {
                if (implementedInterface.Name.Contains(bindableInterface))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string MakeName(string? namespaceName, string namespaceDecl)
    {
        return namespaceDecl + (namespaceName == null ? "" : "." + namespaceName);
        
    }
    public static string? GetFullNamespace(this TypeDeclarationSyntax type)
    {
        string? namespaceName = null;
        var parent = type.Parent;

        while (parent != null)
        {
            switch (parent)
            {
                case NamespaceDeclarationSyntax namespaceDecl:
                    namespaceName = MakeName(namespaceName, namespaceDecl.Name.ToString());
                    break;
                case FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDecl:
                    namespaceName = MakeName(namespaceName, fileScopedNamespaceDecl.Name.ToString());
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