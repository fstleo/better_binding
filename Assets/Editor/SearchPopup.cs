#nullable enable

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BetterBinding.Editor
{
    
    public class SearchPopup : PopupWindowContent
    {

        private readonly List<string> _choices;
        private readonly Action<string> _onResult;
        private readonly Vector2 _size;
        
        private readonly AutocompleteSearchField _autocompleteSearchField;

        public override Vector2 GetWindowSize()
        {
            return _size;
        }

        public SearchPopup(List<string> choices, Action<string> onResult)
        {
            _choices = choices;
            _onResult = onResult;

            _autocompleteSearchField = new AutocompleteSearchField(onInputChanged: FilterResults, onConfirm: OnConfirm);
            
            FilterResults(string.Empty);
            _size = _autocompleteSearchField.GetContentSize();
        }
        
        private static IEnumerable<string> Filter(IEnumerable<string> list, string searchWord)
        {
            if (string.IsNullOrEmpty(searchWord))
            {
                return list;
            }

            var search = searchWord;
            return list.Where(name => name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        private void FilterResults(string input)
        {
            _autocompleteSearchField.ClearResults();
            foreach (var choice in Filter(_choices, input))
            {
                _autocompleteSearchField.AddResult(choice);    
            }
        }

        private void OnConfirm(string obj)
        {
            _onResult.Invoke(obj);
            editorWindow.Close();
        }

        public override void OnOpen()
        {
            _autocompleteSearchField.SetFocus();
            _autocompleteSearchField.SetShowResults();
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(10);
            _autocompleteSearchField.OnGUI();
            
        }
    }
}
#endif
