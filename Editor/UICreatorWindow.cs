using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GOC.UISystem.Editor
{
    public class UICreatorWindow : EditorWindow
    {
        private enum Mode { Screen, Popup }

        private Mode _mode = Mode.Screen;
        private string _name = "";
        private bool _generateData;
        private bool _showHud;
        private bool _usesPlayerInput;
        private string _targetFolder = "Assets/Scripts/UI/Screens";
        private string _dataFolder = "Assets/Scripts/UI/Data";
        private string _generatedFolder = "Assets/Scripts/UI/Generated";
        private string _gameNamespace = "GOC.UISystem";

        private string _enumNamespace = "Game.UI";

        private string ScreenRegistryPath => Path.Combine(_generatedFolder, ".screenregistry.json");
        private string PopupRegistryPath => Path.Combine(_generatedFolder, ".popupregistry.json");
        private string ScreenIdsPath => Path.Combine(_generatedFolder, "ScreenIds.cs");
        private string PopupIdsPath => Path.Combine(_generatedFolder, "PopupIds.cs");

        private Vector2 _scrollPos;

        [MenuItem("Tools/UI System/UI Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<UICreatorWindow>("UI Creator");
            window.minSize = new Vector2(400, 350);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("UI Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _mode = (Mode)GUILayout.Toolbar((int)_mode, new[] { "Create Screen", "Create Popup" });
            EditorGUILayout.Space(8);

            _name = EditorGUILayout.TextField("Name", _name);

            EditorGUILayout.Space(4);
            _generateData = EditorGUILayout.Toggle("Generate Data Struct", _generateData);

            if (_mode == Mode.Screen)
            {
                _showHud = EditorGUILayout.Toggle("Show HUD", _showHud);
                _usesPlayerInput = EditorGUILayout.Toggle("Uses Player Input", _usesPlayerInput);
                _targetFolder = FolderField("Target Folder", _targetFolder);
            }
            else
            {
                if (!_targetFolder.Contains("Popups"))
                    _targetFolder = "Assets/Scripts/UI/Popups";
                _targetFolder = FolderField("Target Folder", _targetFolder);
            }

            _dataFolder = FolderField("Data Folder", _dataFolder);
            _gameNamespace = EditorGUILayout.TextField("Namespace", _gameNamespace);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Generated Files", EditorStyles.boldLabel);
            _generatedFolder = FolderField("Generated Folder", _generatedFolder);
            _enumNamespace = EditorGUILayout.TextField("Enum Namespace", _enumNamespace);

            EditorGUILayout.Space(12);

            string error = Validate();
            if (!string.IsNullOrEmpty(error))
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(error));
            if (GUILayout.Button(_mode == Mode.Screen ? "Create Screen" : "Create Popup", GUILayout.Height(30)))
            {
                if (_mode == Mode.Screen)
                    CreateScreen();
                else
                    CreatePopup();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        private string Validate()
        {
            if (string.IsNullOrWhiteSpace(_name))
                return "Name is required.";

            if (!IsValidIdentifier(_name))
                return "Name must be a valid C# identifier.";

            string registryPath = _mode == Mode.Screen ? ScreenRegistryPath : PopupRegistryPath;
            if (File.Exists(registryPath))
            {
                var registry = EnumCodeGenerator.LoadRegistry(registryPath);
                if (EnumCodeGenerator.HasEntry(registry, _name))
                    return $"A {_mode.ToString().ToLower()} with name '{_name}' already exists.";
            }

            return null;
        }

        private string FolderField(string label, string currentPath)
        {
            EditorGUILayout.BeginHorizontal();
            currentPath = EditorGUILayout.TextField(label, currentPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var picked = EditorUtility.OpenFolderPanel(label, currentPath, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    if (picked.StartsWith(Application.dataPath))
                        currentPath = "Assets" + picked.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();
            return currentPath;
        }

        private bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var provider = CodeDomProvider.CreateProvider("C#");
            return provider.IsValidIdentifier(name);
        }

        private void CreateScreen()
        {
            string suffix = "Screen";
            string className = _name.EndsWith(suffix) ? _name : _name;
            string filePath = Path.Combine(_targetFolder, $"{className}Screen.cs");

            ScreenCodeGenerator.Generate(filePath, className, _gameNamespace, _generateData);

            if (_generateData)
            {
                string dataPath = Path.Combine(_dataFolder, $"{className}Data.cs");
                ScreenCodeGenerator.GenerateData(dataPath, className, _gameNamespace);
            }

            var registry = EnumCodeGenerator.LoadRegistry(ScreenRegistryPath);
            EnumCodeGenerator.AddEntry(registry, className);
            EnumCodeGenerator.SaveRegistry(ScreenRegistryPath, registry);
            EnumCodeGenerator.GenerateScreenIds(ScreenIdsPath, _enumNamespace, registry);

            AddScreenConfigToAsset(className);

            AssetDatabase.Refresh();
            Debug.Log($"[UISystem] Created screen: {className}Screen at {filePath}");
        }

        private void CreatePopup()
        {
            string className = _name;
            string filePath = Path.Combine(_targetFolder, $"{className}Popup.cs");

            PopupCodeGenerator.Generate(filePath, className, _gameNamespace, _generateData);

            if (_generateData)
            {
                string dataPath = Path.Combine(_dataFolder, $"{className}Data.cs");
                PopupCodeGenerator.GenerateData(dataPath, className, _gameNamespace);
            }

            var registry = EnumCodeGenerator.LoadRegistry(PopupRegistryPath);
            EnumCodeGenerator.AddEntry(registry, className);
            EnumCodeGenerator.SaveRegistry(PopupRegistryPath, registry);
            EnumCodeGenerator.GeneratePopupIds(PopupIdsPath, _enumNamespace, registry);

            AssetDatabase.Refresh();
            Debug.Log($"[UISystem] Created popup: {className}Popup at {filePath}");
        }

        private void AddScreenConfigToAsset(string screenKey)
        {
            var guids = AssetDatabase.FindAssets("t:UISystemConfig");
            if (guids.Length == 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var config = AssetDatabase.LoadAssetAtPath<UISystemConfig>(path);
            if (config == null)
                return;

            for (int i = 0; i < config.ScreenConfigs.Count; i++)
            {
                if (config.ScreenConfigs[i].ScreenKey == screenKey)
                    return;
            }

            var screenConfig = new ScreenConfig
            {
                ScreenKey = screenKey,
                ShowHud = _showHud,
                UsesPlayerInput = _usesPlayerInput,
                ExcludeFromHistory = false
            };

            config.ScreenConfigs.Add(screenConfig);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }
}
