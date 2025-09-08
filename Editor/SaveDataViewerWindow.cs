using System.Collections.Generic;
using fefek5.Common.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Variables.SaveDataVariable.Editor
{
    public class SaveDataViewerWindow : EditorWindow
    {
        private ScrollView _jsonScrollView;
        private TextField _searchTextField;
        private Label _counterLabel;
        private string _originalJsonContent;
        private int _currentHighlightIndex;

        private readonly List<VisualElement> _highlightedElements = new();
        private readonly Color _foundColor = new(1, 1, 0, 0.3f);
        private readonly Color _foundSelectedColor = new(1, 0.5f, 0, 0.6f);

        public static void ShowWindow(string jsonContent)
        {
            var window = GetWindow<SaveDataViewerWindow>();
            window.titleContent = new GUIContent("SaveData Viewer");
            window.minSize = new Vector2(400, 300);
            window.SetJsonContent(jsonContent);
        }

        public void SetJsonContent(string jsonContent)
        {
            _originalJsonContent = jsonContent.Replace("\r\n", "\n");
            
            UpdateJsonDisplay();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.marginLeft = 1;
            root.style.marginRight = 1;


            var searchContainer = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 5,
                    minHeight = 30
                }
            };

            _searchTextField = new TextField {
                style = { flexGrow = 1, marginRight = 5 }
            };
            
            _searchTextField.RegisterValueChangedCallback(UpdateJsonDisplay);

            _counterLabel = new Label("0/0") {
                style = {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginRight = 5,
                    minWidth = 50
                }
            };

            var prevButton = new Button(OnPrevButtonClick) { 
                text = "<", 
                tooltip = "Previous", 
                style = { width = 30 } 
            };
            
            var nextButton = new Button(OnNextButtonClick) { 
                text = ">", 
                tooltip = "Next", 
                style = { width = 30 } 
            };

            searchContainer.Add(_searchTextField);
            searchContainer.Add(prevButton);
            searchContainer.Add(_counterLabel);
            searchContainer.Add(nextButton);

            _jsonScrollView = new ScrollView();
            root.Add(searchContainer);
            root.Add(_jsonScrollView);
        }
        
        private void UpdateJsonDisplay(ChangeEvent<string> evt) => UpdateJsonDisplay();

        private void UpdateJsonDisplay()
        {
            _jsonScrollView.Clear();
            _highlightedElements.Clear();
            _currentHighlightIndex = 0;

            if (_originalJsonContent.IsBlank()) return;

            var searchText = _searchTextField.value;
            var lines = _originalJsonContent.Split('\n');

            foreach (var line in lines)
            {
                var lineContainer = new VisualElement {
                    style = {
                        flexDirection = FlexDirection.Row,
                        flexWrap = Wrap.NoWrap,
                        marginLeft = 0,
                        marginRight = 0
                    }
                };

                if (searchText.IsBlank()) AddPreservedWhitespaceLabel(lineContainer, line);
                else
                {
                    var parts = line.Split(new[] { searchText }, System.StringSplitOptions.None);

                    for (var i = 0; i < parts.Length; i++)
                    {
                        if (!parts[i].IsBlank())
                            AddPreservedWhitespaceLabel(lineContainer, parts[i]);

                        if (i >= parts.Length - 1) continue;

                        var highlight = CreateHighlight(searchText);
                        _highlightedElements.Add(highlight);
                        lineContainer.Add(highlight);
                    }
                }

                _jsonScrollView.Add(lineContainer);
            }

            UpdateCounter();
            
            if (_highlightedElements.Count > 0) Navigate(0);
        }

        private static void AddPreservedWhitespaceLabel(VisualElement container, string text)
        {
            var label = new Label(text) {
                style = {
                    whiteSpace = WhiteSpace.Pre,
                    marginLeft = 0,
                    marginRight = 0
                }
            };
            
            container.Add(label);
        }

        private static VisualElement CreateHighlight(string text) =>
            new Label(text) {
                style = {
                    backgroundColor = new Color(1, 1, 0, 0.3f),
                    unityTextAlign = TextAnchor.MiddleLeft,
                    whiteSpace = WhiteSpace.Pre,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0
                }
            };

        private void UpdateCounter()
        {
            var highlightIndex = _currentHighlightIndex >= 0 ? _currentHighlightIndex + 1 : 0;
            _counterLabel.text = $"{highlightIndex}/{_highlightedElements.Count}";
        }

        private void OnPrevButtonClick() => Navigate(-1);

        private void OnNextButtonClick() => Navigate(1);
        
        private void Navigate(int direction)
        {
            if (_highlightedElements.Count == 0) return;

            if (_currentHighlightIndex >= 0)
                _highlightedElements[_currentHighlightIndex].style.backgroundColor = _foundColor;

            _currentHighlightIndex = (_currentHighlightIndex + direction + _highlightedElements.Count) %
                                     _highlightedElements.Count;
            
            var element = _highlightedElements[_currentHighlightIndex];
            element.style.backgroundColor = _foundSelectedColor;
            element.schedule.Execute(Scroll).StartingIn(0);
            
            UpdateCounter();
            
            return;

            void Scroll() => _jsonScrollView.ScrollTo(element);
        }
    }
}