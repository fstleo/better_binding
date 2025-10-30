#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BetterBinding.Editor
{
    public class AutocompleteSearchField
    {
        private static class Styles
        {
            public const float ResultHeight = 20f;
            public const float ResultsBorderWidth = 2f;
            public const float ResultsMargin = 15f;
            public const float ResultsLabelOffset = 2f;
            public const float SearchBarHeight = 18f;

            public static readonly GUIStyle EntryEven;
            public static readonly GUIStyle EntryOdd;
            public static readonly GUIStyle LabelStyle;
            public static readonly GUIStyle ResultsBorderStyle;

            static Styles()
            {
                EntryOdd = new GUIStyle("CN EntryBackOdd");
                EntryEven = new GUIStyle("CN EntryBackEven");
                ResultsBorderStyle = new GUIStyle("hostview");

                LabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    richText = true
                };
            }
        }

        private readonly Action<string> _onInputChanged;
        private readonly Action<string> _onConfirm;
        private string _searchString = string.Empty;
        private const int MaxResults = 15;

        private readonly List<string> _results = new();

        private int _selectedIndex = -1;

        private SearchField? _searchField;

        private Vector2 _previousMousePosition;
        private bool _selectedIndexByMouse;

        private bool _showResults;

        public AutocompleteSearchField(Action<string> onInputChanged, Action<string> onConfirm)
        {
            _onInputChanged = onInputChanged;
            _onConfirm = onConfirm;
        }

        public void AddResult(string result)
        {
            _results.Add(result);
        }

        public void ClearResults()
        {
            _results.Clear();
        }

        public void OnToolbarGUI()
        {
            Draw(asToolbar:true);
        }

        public void OnGUI()
        {
            Draw(asToolbar:false);
        }

        public void SetFocus()
        {
            if (_searchField == null)
            {
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += OnDownOrUpArrowKeyPressed;
            }

            _searchField.SetFocus();
        }

        public void SetShowResults()
        {
            _showResults = true;
        }

        private void Draw(bool asToolbar)
        {
            var rect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            DoSearchField(rect, asToolbar);
            GUILayout.EndHorizontal();
            rect.y += Styles.SearchBarHeight;
            DoResults(rect);
        }

        private void DoSearchField(Rect rect, bool asToolbar)
        {
            if(_searchField == null)
            {
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += OnDownOrUpArrowKeyPressed;
            }

            var result = asToolbar
                ? _searchField.OnToolbarGUI(rect, _searchString)
                : _searchField.OnGUI(rect, _searchString);
    
            if (result != _searchString)
            {
                _onInputChanged(result);
                _selectedIndex = -1;
                _showResults = true;
            }

            _searchString = result;

            if(HasSearchbarFocused())
            {
                RepaintFocusedWindow();
            }
        }

        private void OnDownOrUpArrowKeyPressed()
        {
            var current = Event.current;

            if (current.keyCode == KeyCode.UpArrow)
            {
                current.Use();
                _selectedIndex--;
                _selectedIndexByMouse = false;
            }
            else
            {
                current.Use();
                _selectedIndex++;
                _selectedIndexByMouse = false;
            }

            if (_selectedIndex >= _results.Count) _selectedIndex = _results.Count - 1;
            else if (_selectedIndex < 0) _selectedIndex = -1;
        }

        public Vector2 GetContentSize()
        {
            var bordersWidth = 2 * (Styles.ResultsMargin + Styles.ResultsBorderWidth);
            var sizeY = Styles.SearchBarHeight + Styles.ResultHeight * Mathf.Min(MaxResults, _results.Count) + bordersWidth;
            var sizeX = bordersWidth + (_results.Count == 0 ? 200 : Mathf.Max(200, _results.Max(r => r.Length) * 7));
            return new Vector2(sizeX, sizeY);
        }

        private void DoResults(Rect rect)
        {
            if(!_showResults) return;

            var current = Event.current;
            rect.height = Styles.ResultHeight * Mathf.Min(MaxResults, _results.Count);
            rect.x = Styles.ResultsMargin;
            rect.width -= Styles.ResultsMargin * 2;

            var elementRect = rect;

            rect.height += Styles.ResultsBorderWidth;
            GUI.Label(rect, "", Styles.ResultsBorderStyle);

            var mouseIsInResultsRect = rect.Contains(current.mousePosition);

            if(mouseIsInResultsRect)
            {
                RepaintFocusedWindow();
            }

            var movedMouseInRect = _previousMousePosition != current.mousePosition;

            elementRect.x += Styles.ResultsBorderWidth;
            elementRect.width -= Styles.ResultsBorderWidth * 2;
            elementRect.height = Styles.ResultHeight;

            var didJustSelectIndex = false;
            if (_results.Count == 0)
            {
                if (current.type == EventType.Repaint)
                {
                    Styles.EntryEven.Draw(elementRect, false, false, false, false);
                    var labelRect = elementRect;
                    labelRect.x += Styles.ResultsLabelOffset;
                    GUI.Label(labelRect, "No bindable types found", Styles.LabelStyle);
                }

                elementRect.y += Styles.ResultHeight;
            }

            for (var i = 0; i < _results.Count && i < MaxResults; i++)
            {
                if(current.type == EventType.Repaint)
                {
                    var style = i % 2 == 0 ? Styles.EntryOdd : Styles.EntryEven;

                    style.Draw(elementRect, false, false, i == _selectedIndex, false);

                    var labelRect = elementRect;
                    labelRect.x += Styles.ResultsLabelOffset;
                    GUI.Label(labelRect, _results[i], Styles.LabelStyle);
                }
                if(elementRect.Contains(current.mousePosition))
                {
                    if(movedMouseInRect)
                    {
                        _selectedIndex = i;
                        _selectedIndexByMouse = true;
                        didJustSelectIndex = true;
                    }
                    if(current.type == EventType.MouseDown)
                    {
                        OnConfirm(_results[i]);
                    }
                }
                elementRect.y += Styles.ResultHeight;
            }

            if(current.type == EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && _selectedIndexByMouse)
            {
                _selectedIndex = -1;
            }

            if((GUIUtility.hotControl != _searchField?.searchFieldControlID && GUIUtility.hotControl > 0)
               || (current.rawType == EventType.MouseDown && !mouseIsInResultsRect))
            {
                _showResults = false;
            }

            if(current.type == EventType.KeyUp && current.keyCode == KeyCode.Return && _selectedIndex >= 0)
            {
                OnConfirm(_results[_selectedIndex]);
            }

            if(current.type == EventType.Repaint)
            {
                _previousMousePosition = current.mousePosition;
            }
        }

        private void OnConfirm(string result)
        {
            _searchString = result;
            _onInputChanged(result);
            _onConfirm(result);
            RepaintFocusedWindow();
            GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
        }

        private bool HasSearchbarFocused()
        {
            return GUIUtility.keyboardControl == _searchField?.searchFieldControlID;
        }

        private static void RepaintFocusedWindow()
        {
            if(EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }
    }
}
#endif
