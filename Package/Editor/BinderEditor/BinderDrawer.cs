#nullable enable

using System.Linq;
using BetterBinding.Editor.Utils;
using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using UnityEditor;
using UnityEngine;

namespace BetterBinding.Editor.BinderEditor
{
    [CustomEditor(typeof(Binder))]
    public class BinderDrawer : UnityEditor.Editor
    {
        private ContractEditorVm? _vm;
        private GUIStyle? _addButtonStyle;
        private GUIStyle? _deleteButtonStyle;
        private GUIStyle? _bgStyle;
        private GUIStyle? _headerStyle;
        private GUIStyle? _bindingNameStyle;

        private GUIStyleState? _emptyButtonState;

        private void OnEnable()
        {
            var emptyTexture = new Texture2D(1, 1);
            emptyTexture.SetPixels(new[] { Color.clear });
            emptyTexture.Apply();
            
            _emptyButtonState = new GUIStyleState
            {
                background = emptyTexture
            };
            var addButtonTexture = EditorGUIUtility.IconContent("d_CollabCreate Icon").image;

            _addButtonStyle = new GUIStyle
            {
                normal = { background = addButtonTexture as Texture2D }, fixedWidth = 30, fixedHeight = 30
            };

            var deleteButtonTexture = EditorGUIUtility.IconContent("d_CollabDeleted Icon").image;
            _deleteButtonStyle = new GUIStyle
            {
                normal = { background = deleteButtonTexture as Texture2D }, fixedWidth = 25, fixedHeight = 25
            };
        
            _vm = new ContractEditorVm(serializedObject.FindProperty("_contractId"), serializedObject.FindProperty("_serializedBindings"));
            _vm.PossibleContracts.AddRange(BindableClassesUtils.ContractsByName.Keys);
        }

        private void OnDisable()
        {
            _vm?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            if (_vm == null)
            {
                return;
            }
            
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fixedHeight = 60,
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter
            };
            _bindingNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fixedHeight = 30,
                alignment = TextAnchor.UpperLeft
            };
            EditorGUILayout.BeginVertical();
            var rect = EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Set contract type"))
            {
                PopupWindow.Show(rect, new SearchPopup(_vm.PossibleContracts, _vm.SetContractCommand.Execute));
            }

            EditorGUILayout.EndVertical();

            if (!string.IsNullOrEmpty(_vm.ContractName))
            {
                if (GUILayout.Button(_vm.ContractName, _headerStyle))
                {
                    _vm.OpenContractCommand.Execute();
                }

                foreach (var binding in _vm.Bindings)
                {
                    DrawBinding(binding);
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBinding(BindingVm binding)
        {
            var bgRect = EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            var bindingNameRect = EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(800));

            DrawDropdown(binding, bindingNameRect);

            var rect = new Rect(bindingNameRect.x, bindingNameRect.y, bgRect.width, bindingNameRect.height);
            DrawAddButton(binding, rect);

            DrawName(binding);

            EditorGUILayout.EndHorizontal();

            DrawBindings(binding);

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            DrawBackground(bgRect);
            if (binding.CanBeFoldout &&
                EditorGUI.DropdownButton(bgRect, GUIContent.none, FocusType.Passive, _bgStyle))
            {
                binding.ToggleFoldoutCommand.Execute();
            }
        }

        private void DrawBackground(Rect bgRect)
        {
            _bgStyle = new GUIStyle(EditorStyles.colorField)
            {
                normal = _emptyButtonState,
                active = _emptyButtonState,
                hover = _emptyButtonState,
                focused = _emptyButtonState,
                onNormal = _emptyButtonState,
                onActive = _emptyButtonState,
                onHover = _emptyButtonState,
                onFocused = _emptyButtonState,
            };

            EditorGUI.HelpBox(bgRect, string.Empty, MessageType.None);
        }

        private void DrawName(BindingVm binding)
        {
            EditorGUILayout.LabelField(binding.Name, _bindingNameStyle);
        }

        private void DrawBindings(BindingVm binding)
        {
            if (!binding.Foldout)
            {
                return;
            }
        
            EditorGUI.indentLevel++;
            for (var index = 0; index < binding.BindingsArray.arraySize; index++)
            {
                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (GUILayout.Button(string.Empty, _deleteButtonStyle)
                    && EditorUtility.DisplayDialog("Delete binding", "Are you sure?", "Yes", "No"))
                {
                    binding.RemoveBindingCommand.OnNext(index);
                    index--;
                    continue;
                }

                var property = binding.BindingsArray.GetArrayElementAtIndex(index).Copy();
                if (property.managedReferenceValue != null)
                {
                    var end = property.GetEndProperty();
                    var propertyRect = EditorGUILayout.BeginVertical();
                    while (property.NextVisible(true) && !SerializedProperty.EqualContents(property, end))
                    {
                        EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
                    }
                    
                    EditorGUILayout.EndVertical();
                    propertyRect = new Rect(propertyRect.x, propertyRect.y - 5, propertyRect.width + 20,
                        propertyRect.height + 10);
                    DrawBackground(propertyRect);
                }
            
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }

        private void DrawAddButton(BindingVm binding, Rect rect)
        {
            GUILayout.Space(5);

            if (GUILayout.Button(string.Empty, _addButtonStyle))
            {
                PopupWindow.Show(rect,
                    new SearchPopup(binding.PossibleTypesList.Select(type => type.Name).ToList(),
                        binding.AddBindingCommand.Execute));
            }
        }

        private static void DrawDropdown(BindingVm binding, Rect rect)
        {
            if (binding.CanBeFoldout)
            {
                EditorGUI.Foldout(rect, binding.Foldout, GUIContent.none);
            }
        }
    
    }
}