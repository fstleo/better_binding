// #nullable enable
//
// using System.Linq;
// using BetterBinding.CodeGen.Utils;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;
//
// namespace BetterBinding.CodeGen;
//
// [Generator]
// public class BindingsGenerator : ISourceGenerator
// {
//     private string _componentTemplate = """
//                                         #nullable enable
//
//                                         using System;
//                                         using UnityEngine;
//                                         {namespace}
//
//                                         namespace BetterBinding.Runtime
//                                         {
//                                             [Serializable]
//                                             public class {binding-name}Binding : PropertyBinding<{value-type}>
//                                             {
//                                                 [SerializeField]
//                                                 private {component-class} _{component-class} = null!;
//                                                 
//                                                 protected override void SetValue({value-type}? value)
//                                                 {
//                                                     {call};
//                                                 }
//                                             }
//                                         }
//                                         """;
//     public void Initialize(GeneratorInitializationContext context)
//     {
//         context.RegisterForSyntaxNotifications(() => new ComponentsCollector());
//     }
//
//     public void Execute(GeneratorExecutionContext context)
//     {
//         var componentsCollector = (ComponentsCollector)context.SyntaxReceiver!;
//         
//         context.AddSource("ComponentsCount.g.cs", $"//{componentsCollector.WorkItems.Count}");
//         foreach (var componentType in componentsCollector.WorkItems)
//         {
//                 var namespaceName = componentType.GetFullNamespace();
//                 var componentClass = componentType.Identifier.Text;
//                 string? bindingName = null;
//                 string? valueType = null;
//                 string? call = null;
//                 if (member is PropertyDeclarationSyntax property)
//                 {
//                     bindingName = componentType.Identifier + property.Identifier.Text;
//                     valueType = property.Type.ToString();
//                     call = $"_{componentType}.{property.Identifier.Text} = value";
//                 }
//
//                 if (member is FieldDeclarationSyntax field)
//                 {
//                     foreach (var fieldName in field.Declaration.Variables
//                                  .Select(fieldVariable => fieldVariable.Identifier.Text))
//                     {
//                         bindingName = componentType.Identifier + fieldName;
//                         valueType = field.Declaration.Type.ToString();
//                         call = $"_{componentType}.{fieldName} = value";
//                     }
//                 }
//                     
//                 if (member is MethodDeclarationSyntax method && method.ParameterList.Parameters.Count > 0)
//                 {
//                     bindingName = componentType.Identifier + method.Identifier.Text;
//                     valueType = method.ParameterList.Parameters[0].Type?.ToString();
//                     call = $"_{componentType}.{method.Identifier.Text}(value)";
//                 }
//
//                 if (call == null || bindingName == null || valueType == null)
//                 {
//                     return;
//                 }
//
//                 var result = _componentTemplate
//                     .Replace("{namespace}", namespaceName)
//                     .Replace("{binding-name}", componentType.Identifier.Text + member.Modifiers)
//                     .Replace("{value-type}", valueType)
//                     .Replace("{call}", call)
//                     .Replace("{component-class}", componentClass);
//                 context.AddSource($"{bindingName}.g.cs", result);
//         }
//
//     }
// }