#nullable enable

using System;
using System.Collections.Generic;
using BetterBinding.Runtime;
using UnityEditor;
using UnityEngine;

namespace BetterBinding.Editor
{
    public class ContractEditorVm
    {
        public string ContractName { get; private set; } = string.Empty;
        public List<string> PossibleContracts { get; } = new ();
        public List<BindingVm> Bindings { get; } = new();
        public Property<Unit> OpenContractCommand { get; } = new();
        public Property<string> SetContractCommand { get; } = new();
        
        public ContractEditorVm(SerializedProperty contractIdProperty, SerializedProperty bindingsProperty)
        {
            if (BinderHelper.ContractsById.TryGetValue(contractIdProperty.ulongValue, out var contractName))
            {
                ContractName = contractName;
            }
        
            SetContractCommand.Changed += newContractName =>
            {
                if (ContractName.Equals(newContractName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                Debug.LogError("Set contract name to " + newContractName);
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
            };

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
                    var propertyId = serializedBinding.FindPropertyRelative("Id").ulongValue;
                    if (propertyId != bindableProperty.Key)
                    {
                        continue;
                    }
                    
                    serializedBindingForProperty = serializedBinding;
                    break;
                }

                if (serializedBindingForProperty == null)
                {
                    bindingsProperty.arraySize++;
                    serializedBindingForProperty = bindingsProperty.GetArrayElementAtIndex(bindingsProperty.arraySize - 1);
                    serializedBindingForProperty.FindPropertyRelative("Id").ulongValue = bindableProperty.Key;
                    bindingsProperty.serializedObject.ApplyModifiedProperties();
                }
            
                Bindings.Add(new BindingVm(serializedBindingForProperty, bindableProperty.Value));
            }
        
        }
    }
}