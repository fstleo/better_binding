#nullable enable

using System;
using System.Collections.Generic;
using BetterBinding.Runtime;
using UnityEditor;

namespace BetterBinding.Editor
{
    public class BindingVm
    {
        public bool CanBeFoldout { get; private set; }
        public Property<Unit> ToggleFoldoutCommand { get; } = new();
        public bool Foldout { get; private set; }
        public string Name { get; private set; }
        public Property<int> RemoveViewCommand { get; } = new();
        public List<Type> PossibleTypesList { get; } = new();
        public Property<string> AddBinding { get; } = new();
        public SerializedProperty BindingsArray { get; } 

        public BindingVm(SerializedProperty property, (string Name, Type Type) propertyInfo)
        {
            Name = propertyInfo.Name;
            foreach (var bindableType in BinderHelper.GetBindableTypesFor(propertyInfo.Type))
            {
                PossibleTypesList.Add(bindableType);
            }

            BindingsArray = property.FindPropertyRelative("Bindings");
            CanBeFoldout = BindingsArray.arraySize > 0;
            AddBinding.Changed += bindableTypeName =>
            {
                if (string.IsNullOrEmpty(bindableTypeName))
                {
                    return;
                }

                foreach (var type in PossibleTypesList)
                {
                    if (type.Name.Equals(bindableTypeName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        BindingsArray.InsertArrayElementAtIndex(BindingsArray.arraySize);
                        CreateInstance(BindingsArray.GetArrayElementAtIndex(BindingsArray.arraySize - 1), type);
                    
                        CanBeFoldout = BindingsArray.arraySize > 0;
                    }
                }
            };
        
            ToggleFoldoutCommand.Changed += _ =>
            {
                if (CanBeFoldout)
                {
                    Foldout = !Foldout;
                }
            };

            RemoveViewCommand.Changed += index =>
            {
                BindingsArray.DeleteArrayElementAtIndex(index);
                BindingsArray.serializedObject.ApplyModifiedProperties();
                CanBeFoldout = BindingsArray.arraySize > 0;
            };
        }
    
        private static void CreateInstance(SerializedProperty property, Type type)
        {
            var target = Activator.CreateInstance(type);
            property.managedReferenceValue = target;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}