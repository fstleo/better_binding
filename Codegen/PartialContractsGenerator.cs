#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BetterBinding.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BetterBinding.CodeGen
{
    [Generator]
    public class PartialContractsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxCollector());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            var moduleFullName = context.Compilation.SourceModule.Name;
            if (moduleFullName.StartsWith("UnityEngine.")) return;
            if (moduleFullName.StartsWith("UnityEditor.")) return;
            if (moduleFullName.StartsWith("Unity.")) return; 
            var syntaxCollector = (SyntaxCollector)context.SyntaxReceiver!;
            
            var contractsList = new List<(TypeDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol namedTypeSymbol)>();
            foreach (var propertiesOwner in syntaxCollector.WorkItems)
            {
                var semanticModel = context.Compilation.GetSemanticModel(propertiesOwner.SyntaxTree);
                var symbol = semanticModel.GetDeclaredSymbol(propertiesOwner);
                if (symbol is not INamedTypeSymbol namedSymbol)
                {
                    continue;
                }
                
                contractsList.Add((propertiesOwner, namedSymbol));
            }

            foreach (var (propertiesOwner, namedSymbol) in contractsList)
            {
                var code = GenerateForType(propertiesOwner, namedSymbol, 
                    context.Compilation.GetSemanticModel(propertiesOwner.SyntaxTree));
                context.AddSource($"{propertiesOwner.Identifier.Text}.g.cs", code);
            }
            
            foreach (var collectionType in syntaxCollector.CollectionsToCreate)
            {
                var collectionWriter = new CodeWriter();
                collectionWriter.AppendLine("#nullable enable");
                collectionWriter.AppendLine();
                foreach (var dependency in collectionType.Usings)
                {
                    collectionWriter.AppendLine($"using {dependency};");
                }
                
                collectionWriter.AppendLine();
 
                var genericParametersStartIndex = collectionType.ClassName.IndexOf("<", StringComparison.OrdinalIgnoreCase);
                if (genericParametersStartIndex < 0)
                {
                    continue;
                }
                
                var genericParameters = collectionType.ClassName.Substring(genericParametersStartIndex);
                genericParameters = genericParameters.Substring(1, genericParameters.Length - 2);
                
                var className = collectionType.ClassName.Substring(0, genericParametersStartIndex) 
                                + genericParameters;
                using (collectionWriter.BeginBlockScope($"namespace {collectionType.Namespace}"))
                {
                    using (collectionWriter.BeginBlockScope(
                               $"public class {genericParameters}CollectionBinding : CollectionBinding<{genericParameters}>"))
                    {
                    }
                }
                
                context.AddSource($"{className}Binding.g.cs", collectionWriter.ToString());
            }
        }

        private static string GenerateForType(TypeDeclarationSyntax viewModel,
            INamedTypeSymbol namedSymbol, 
            SemanticModel semanticModel)
        {
            var containingNamespace = viewModel.GetFullNamespace();
            var contractWriter = new CodeWriter();
            contractWriter.AppendLine("#nullable enable");
            contractWriter.AppendLine();
            contractWriter.AppendLine("using BetterBinding.Runtime;");
            contractWriter.AppendLine("using System;");
            contractWriter.AppendLine("using System.Collections.Generic;");
            contractWriter.AppendLine("using System.Diagnostics.CodeAnalysis;");
            
            contractWriter.AppendLine();
            IDisposable? namespaceScope = null;
            if (!string.IsNullOrWhiteSpace(containingNamespace))
            {
                namespaceScope = contractWriter.BeginBlockScope($"namespace {containingNamespace}");
            }
            
            contractWriter.AppendLine();

            var classDeclaration = MakeClassDeclaration(viewModel);
            using (contractWriter.BeginBlockScope($"{classDeclaration}"))
            {
                var bindableProperties = viewModel.GetBindableProperties(semanticModel).ToArray();
                GenerateTryGetPropertiesMethod(contractWriter, bindableProperties);
                contractWriter.AppendLine();
                GenerateContractsList(namedSymbol, contractWriter);
                contractWriter.AppendLine();
                GenerateDispose(contractWriter, viewModel, bindableProperties);
            }
            
            contractWriter.AppendLine();
            namespaceScope?.Dispose();
            return contractWriter.ToString();
        }

        private static string MakeClassDeclaration(TypeDeclarationSyntax viewModel)
        {
            var declarationBuilder = new StringBuilder();
            var modifiers = viewModel.Modifiers.Select(modifier => modifier.Text);
            foreach (var modifier in modifiers)
            {
                declarationBuilder.Append(modifier + " ");
            }

            var typeParameters = string.Empty;
            if (viewModel.TypeParameterList != null)
            {
                typeParameters = string.Join(", ", viewModel.TypeParameterList.Parameters.Select(p => p.Identifier.Text));
                typeParameters = $"<{typeParameters}>";
            }
            
            declarationBuilder.Append($"class {viewModel.Identifier.Text}{typeParameters} : IViewModel");
            if (viewModel.BaseList == null 
                || viewModel.BaseList.Types.All(type => type.ToString() != nameof(IDisposable)))
            {
                declarationBuilder.Append($", {nameof(IDisposable)}");
            }
            
            return declarationBuilder.ToString();
        }

        private static void GenerateContractsList(INamedTypeSymbol namedSymbol, CodeWriter contractWriter)
        {
            using (contractWriter.BeginBlockScope("public bool ImplementsContract(ulong contractId)"))
            {
                using (contractWriter.BeginBlockScope("switch (contractId)"))
                {
                    var implementedContracts = namedSymbol.GetAllContracts();
                    foreach (var contract in implementedContracts)
                    {
                        var hash = CalculateHash(contract.Name);
                        contractWriter.AppendLine($"case {hash}:");
                    }
                        
                    contractWriter.AppendLine("return true;");
                }
                        
                contractWriter.AppendLine("return false;");
            }
        }

        private static void GenerateTryGetPropertiesMethod(CodeWriter contractWriter, PropertyDeclarationSyntax[] bindableProperties)
        {
            using (contractWriter.BeginBlockScope("public bool TryGetProperty(ulong id, [NotNullWhen(true)] out object? value)"))
            {
                //TODO: Make it Dictionary for many (>5?) properties
                using (contractWriter.BeginBlockScope("switch (id)"))
                {
                    foreach (var property in bindableProperties)
                    {
                        var propertyName = property.Identifier.ToString();
                        var propertyHash = CalculateHash(propertyName);
                        contractWriter.AppendLine($"case {propertyHash}: {{ value = {propertyName}; return true; }}");
                    }
                    
                    contractWriter.AppendLine("default: { value = null; return false; }");
                }
            }
        }

        private static void GenerateDispose(CodeWriter contractWriter, TypeDeclarationSyntax viewModel, 
            PropertyDeclarationSyntax[] bindableProperties)
        {
            if (!viewModel.Members.Any(member => member
                    is MethodDeclarationSyntax method && method.Identifier.Text
                    .Equals("Dispose", StringComparison.InvariantCultureIgnoreCase)))
            {
                using (contractWriter.BeginBlockScope("public void Dispose()"))
                {
                    contractWriter.AppendLine("DisposeInternal();");
                }
            }
            
            contractWriter.AppendLine();
            using (contractWriter.BeginBlockScope("private void DisposeInternal()"))
            {
                foreach (var property in bindableProperties)
                {
                    var propertyName = property.Identifier.ToString();
                    contractWriter.AppendLine($"{propertyName}.Dispose();");
                }
            }
        }
        
        //TODO: move to a separate lib
        private static ulong CalculateHash(string read)
        {
            var hashedValue = 3074457345618258791ul;
            foreach (var t in read)
            {
                hashedValue += t;
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}