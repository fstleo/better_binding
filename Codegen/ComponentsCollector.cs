// using System;
// using System.Collections.Generic;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using SyntaxKind = Microsoft.CodeAnalysis.VisualBasic.SyntaxKind;
//
// namespace BetterBinding.CodeGen;
//
// public struct PropertyToGenerate
// {
//     public string Namespace;
//     public string ClassName;
//     public string BindingName;
// }
//
// public class ComponentsCollector : ISyntaxReceiver
// {
//     public List<PropertyToGenerate> WorkItems { get; } = new();
//
//     private static bool HasComponentBaseClass(INamedTypeSymbol type)
//     {
//         while (type.BaseType is not null)
//         {
//             if (type.BaseType.Name.Contains("MonoBeh"))
//             {
//                 return true;
//             }
//
//             type = type.BaseType;
//         }
//
//         return false;
//     }
//
//     public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//     {
//         // var semanticModel = context.SemanticModel;
//         // if (context.Node is not TypeDeclarationSyntax type)
//         // {
//         //     return;
//         // }
//         //     
//         // var symbol = semanticModel.GetDeclaredSymbol(type);
//         if (syntaxNode is not TypeDeclarationSyntax type || !type.Identifier.Text.Contains("Generate")) return;
//         foreach (var attribute in type.AttributeLists)
//         {
//             foreach (var attr in attribute.Attributes)
//             {
//                 if (!attr.Name.ToString().Contains("Generate") || attr.ArgumentList == null)
//                 {
//                     continue;
//                 }
//                     
//                 WorkItems.Add(new PropertyToGenerate
//                 {
//                     Namespace = RetrieveString(attr.ArgumentList.Arguments[0]),
//                     ClassName = RetrieveString(attr.ArgumentList.Arguments[1]),
//                     PropertyName = RetrieveString(attr.ArgumentList.Arguments[2]),
//                 });
//             }
//         }
//     }
//     
//     private string RetrieveString(AttributeArgumentSyntax argument)
//     {
//         if (argument.Expression is LiteralExpressionSyntax literal &&
//             literal.IsKind(SyntaxKind.StringLiteralExpression))
//         {
//             return literal.Token.ValueText;
//         }
//         
//         return string.Empty;
//     }
// }