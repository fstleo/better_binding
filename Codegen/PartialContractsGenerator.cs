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
                var code = GenerateForType(propertiesOwner, namedSymbol);
                context.AddSource($"{propertiesOwner.Identifier.Text}.g.cs", code);
            }
        }

        private static string GenerateForType(TypeDeclarationSyntax viewModel, INamedTypeSymbol namedSymbol)
        {
            var containingNamespace = viewModel.GetFullNamespace();
            var contractWriter = new CodeWriter();
            contractWriter.AppendLine("#nullable enable");
            contractWriter.AppendLine();
            contractWriter.AppendLine("using BetterBinding.Runtime;");
            contractWriter.AppendLine("using System;");
            contractWriter.AppendLine("using System.Collections.Generic;");
            contractWriter.AppendLine("using System.Diagnostics.CodeAnalysis;");
            
            IDisposable? namespaceScope = null;
            if (!string.IsNullOrWhiteSpace(containingNamespace))
            {
                namespaceScope = contractWriter.BeginBlockScope($"namespace {containingNamespace}");
            }
            contractWriter.AppendLine();
            foreach (var parameter in viewModel.Modifiers)
            {
                contractWriter.AppendLine("//" + parameter.Text);
            }

            var declarationBuilder = new StringBuilder();
            var modifiers = viewModel.Modifiers.Select(modifier => modifier.Text);
            foreach (var modifier in modifiers)
            {
                declarationBuilder.Append(modifier + " ");
            }

            declarationBuilder.Append($"class {viewModel.Identifier.Text} : IViewModel");
            if (viewModel.BaseList == null 
                || viewModel.BaseList.Types.All(type => type.ToString() != nameof(IDisposable)))
            {
                declarationBuilder.Append($", {nameof(IDisposable)}");
            }
            
            using (contractWriter.BeginBlockScope($"{declarationBuilder}"))
            {
                GeneratePropertiesList(contractWriter, viewModel);
                contractWriter.AppendLine();
                GenerateContractsList(namedSymbol, contractWriter);
                contractWriter.AppendLine();
            }
            
            namespaceScope?.Dispose();
            return contractWriter.ToString();
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

        private static void GeneratePropertiesList(CodeWriter contractWriter, TypeDeclarationSyntax viewModel)
        {
            var bindableProperties = viewModel.GetBindableProperties().ToArray();
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
            
            contractWriter.AppendLine();
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