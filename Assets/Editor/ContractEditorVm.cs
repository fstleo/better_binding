#nullable enable

using System;
using System.Collections.Generic;
using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using UnityEditor;

namespace BetterBinding.Editor
{
    public partial class ContractEditorVm : IDisposable
    {
        public string ContractName { get; private set; } = string.Empty;
        public List<string> PossibleContracts { get; } = new();
        public List<BindingVm> Bindings { get; } = new();
        public Property<Unit> OpenContractCommand { get; } = Property<Unit>.Command();
        public Property<string> SetContractCommand { get; } = Property<string>.Command();

        public ContractEditorVm(SerializedProperty contractIdProperty, SerializedProperty bindingsProperty)
        {
            if (BinderHelper.ContractsById.TryGetValue(contractIdProperty.ulongValue, out var contractName))
            {
                ContractName = contractName;
            }

            SetContractCommand.Subscribe(newContractName =>
            {
                if (ContractName.Equals(newContractName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                if (newContractName.IsNullOrEmpty()
                    || !BinderHelper.ContractsByName.TryGetValue(newContractName, out var contractId))
                {
                    return;
                }

                bindingsProperty.arraySize = 0;
                bindingsProperty.serializedObject.ApplyModifiedProperties();
                contractIdProperty.ulongValue = contractId;
                contractIdProperty.serializedObject.ApplyModifiedProperties();
                ContractName = newContractName;
                RecreateBindings(contractIdProperty.ulongValue, bindingsProperty);
            });

            RecreateBindings(contractIdProperty.ulongValue, bindingsProperty);
        }

        private void RecreateBindings(ulong contractId, SerializedProperty bindingsProperty)
        {
            if (BinderHelper.PropertiesByContracts.TryGetValue(contractId, out var properties))
            {
                CreateBindings(bindingsProperty, properties);
            }
        }

        private void CreateBindings(SerializedProperty bindingsProperty, Dictionary<ulong,
            (string Name, Type Type)> propertiesNames)
        {
            foreach (var bindableProperty in propertiesNames)
            {
                SerializedProperty? serializedBindingForProperty = null;
                for (var i = 0; i < bindingsProperty.arraySize; i++)
                {
                    var serializedBinding = bindingsProperty.GetArrayElementAtIndex(i);
                    var propertyId = serializedBinding.FindPropertyRelative(nameof(Binder.SerializedBinding.Id))
                        .ulongValue;
                    if (propertyId != bindableProperty.Key)
                    {
                        continue;
                    }

                    serializedBindingForProperty = serializedBinding;
                    break;
                }

                serializedBindingForProperty ??= CreateMissingBinding(bindingsProperty, bindableProperty);

                Bindings.Add(new BindingVm(serializedBindingForProperty, bindableProperty.Value));
            }
        }

        private static SerializedProperty CreateMissingBinding(SerializedProperty bindingsProperty,
            KeyValuePair<ulong, (string Name, Type Type)> bindableProperty)
        {
            bindingsProperty.arraySize++;
            var serializedBindingForProperty = bindingsProperty.GetArrayElementAtIndex(bindingsProperty.arraySize - 1);
            serializedBindingForProperty.FindPropertyRelative(nameof(Binder.SerializedBinding.Id)).ulongValue =
                bindableProperty.Key;
            serializedBindingForProperty.FindPropertyRelative(nameof(Binder.SerializedBinding.Bindings)).arraySize = 0;
            bindingsProperty.serializedObject.ApplyModifiedProperties();
            return serializedBindingForProperty;
        }

        public void Dispose()
        {
            // TODO: bindable collections
            foreach (var binding in Bindings)
            {
                binding.Dispose();
            }
            
            Bindings.Clear();
            
            DisposeInternal();
        }
    }
}