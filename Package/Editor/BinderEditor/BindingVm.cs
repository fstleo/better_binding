#nullable enable

using System;
using System.Collections.Generic;
using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using BetterBinding.Editor.Utils;
using UnityEditor;

namespace BetterBinding.Editor.BinderEditor
{
    [HideInBinder]
    public partial class BindingVm
    {
        public bool CanBeFoldout { get; private set; }
        public Property<Unit> ToggleFoldoutCommand { get; } = Property<Unit>.Command();
        public bool Foldout { get; private set; }
        public string Name { get; private set; }
        public Property<int> RemoveBindingCommand { get; } = Property<int>.Command();
        public List<Type> PossibleTypesList { get; } = new();
        public Property<string> AddBindingCommand { get; } = Property<string>.Command();
        public SerializedProperty BindingsArray { get; } 

        public BindingVm(SerializedProperty property, (string Name, Type Type) propertyInfo)
        {
            Name = propertyInfo.Name;
            foreach (var bindableType in BindableClassesUtils.GetBindableTypesFor(propertyInfo.Type))
            {
                PossibleTypesList.Add(bindableType);
            }

            BindingsArray = property.FindPropertyRelative(nameof(Binder.SerializedBinding.Bindings));
            CanBeFoldout = BindingsArray.arraySize > 0;
            AddBindingCommand.Subscribe(AddBinding);
            ToggleFoldoutCommand.Subscribe(ToggleFoldout);
            RemoveBindingCommand.Subscribe(RemoveBinding);
        }

        private void RemoveBinding(int index)
        {
            BindingsArray.DeleteArrayElementAtIndex(index);
            BindingsArray.serializedObject.ApplyModifiedProperties();
            CanBeFoldout = BindingsArray.arraySize > 0;
        }

        private void ToggleFoldout(Unit _)
        {
            if (CanBeFoldout)
            {
                Foldout = !Foldout;
            }
        }

        private void AddBinding(string? bindableTypeName)
        {
            if (string.IsNullOrEmpty(bindableTypeName))
            {
                return;
            }

            foreach (var type in PossibleTypesList)
            {
                if (!type.Name.Equals(bindableTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                BindingsArray.InsertArrayElementAtIndex(BindingsArray.arraySize);
                CreateInstance(BindingsArray.GetArrayElementAtIndex(BindingsArray.arraySize - 1), type);
                
                CanBeFoldout = BindingsArray.arraySize > 0;
                if (!Foldout)
                {
                    Foldout = true;
                }
            }
        }

        private static void CreateInstance(SerializedProperty property, Type type)
        {
            var target = Activator.CreateInstance(type);
            property.managedReferenceValue = target;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}