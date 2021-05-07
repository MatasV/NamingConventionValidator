using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using ReorderableList = UnityEditorInternal.ReorderableList;

namespace NamingValidator
{
    public sealed class NamingConventionValidator : EditorWindow
    {
        private bool _running = false;
        private Stopwatch stopwatch;

        private ReorderableList _reorderableList;

        public List<CustomNamingValidator> customNamingValidators;
        private List<string> folderPaths;
        private Rect[] rects;

        public static List<Object> checkedGOs;
        
        [MenuItem("Tools/Naming Convention Validator 📃")]
        static void Init()
        {
            NamingConventionValidator window =
                (NamingConventionValidator) EditorWindow.GetWindow(typeof(NamingConventionValidator), focusedWindow,
                    "Naming Convention Validator");
            
            window.Show();
        }

        #region Editor Drawing
        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Label("Basic Options", EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space();

            NamingConventionValidatorDatabase.CapitalizationConv =
                (NamingConventionValidatorDatabase.CapitalizationConvention) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Capitalization Convention",
                        "Checks if object names follow the desired capitalization convention."),
                    NamingConventionValidatorDatabase.CapitalizationConv);

            NamingConventionValidatorDatabase.SpacingConv =
                (NamingConventionValidatorDatabase.SpacingConvention) EditorGUILayout.EnumPopup(new GUIContent(
                        "Spacing Convention",
                        "Checks if object names follow the desired spacing convention."),
                    NamingConventionValidatorDatabase.SpacingConv);

            NamingConventionValidatorDatabase.CheckForParenthesis = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Check for Parenthesis",
                    "Checks for pattern: \"(\"-\"NUMBER\"-\")\" to find any leftover default naming from duplicating objects, e.g., GameObject(2)."),
                NamingConventionValidatorDatabase.CheckForParenthesis);

            NamingConventionValidatorDatabase.CheckForDefaultNames = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Check for Default Names",
                    "Checks for objects named automatically when created by GameObject creation menu."),
                NamingConventionValidatorDatabase.CheckForDefaultNames);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Spell Check", EditorStyles.whiteLabel);
            EditorGUILayout.Space();
            NamingConventionValidatorDatabase.TextFieldSpellCheck = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Spell Check Text Components",
                    "Spell checks Text and TMP_Text Components on objects"),
                NamingConventionValidatorDatabase.TextFieldSpellCheck);

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Profanity Check", EditorStyles.whiteLabel);
            EditorGUILayout.Space();

            NamingConventionValidatorDatabase.ProfanityCheckTextfield = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Profanity Check Text Components",
                    "Profanity checks Text and TMP_Text Components on objects"),
                NamingConventionValidatorDatabase.ProfanityCheckTextfield);

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            NamingConventionValidatorDatabase.CustomValidatorsEnabled = EditorGUILayout.BeginToggleGroup(new GUIContent(
                "Custom Validators Enabled",
                "Enable custom validators"), NamingConventionValidatorDatabase.CustomValidatorsEnabled);

            if (NamingConventionValidatorDatabase.CustomValidatorsEnabled)
            {
                customNamingValidators = NamingConventionValidatorDatabase.CustomNamingValidators;
                
                ScriptableObject target = this;
                SerializedObject so = new SerializedObject(target);
                SerializedProperty stringsProperty = so.FindProperty("customNamingValidators");

                EditorGUILayout.PropertyField(stringsProperty, true);

                so.ApplyModifiedProperties();

                NamingConventionValidatorDatabase.CustomNamingValidators = customNamingValidators; 
            }

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();
            NamingConventionValidatorDatabase.ShouldCheckFolders = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Check in Folders",
                    "Should look in folders to validate names"),
                NamingConventionValidatorDatabase.ShouldCheckFolders);
            if (NamingConventionValidatorDatabase.ShouldCheckFolders)
            {
                EditorGUILayout.BeginVertical();

                folderPaths = NamingConventionValidatorDatabase.FolderPaths;
                
                if (folderPaths.Count == 0)
                {
                    folderPaths.Add(string.Empty);
                }

                if (folderPaths[folderPaths.Count - 1] == string.Empty && folderPaths.Count > 1)
                {
                    if (folderPaths[folderPaths.Count - 2] == string.Empty)
                        folderPaths.Remove(folderPaths[folderPaths.Count - 1]);
                }
                else if (folderPaths[folderPaths.Count - 1] != string.Empty && folderPaths.Count > 0)
                {
                    folderPaths.Add(string.Empty);
                }
                rects = new Rect[folderPaths.Count];

                for (var index = 0; index < folderPaths.Count; index++)
                {
                    rects[index] = EditorGUILayout.GetControlRect(GUILayout.Width(300));
                    folderPaths[index] = EditorGUI.TextField(rects[index], folderPaths[index]);

                    //If the mouse is being dragged or at the end of the drag, and the mouse position is in the text input box  
                    if ((Event.current.type == EventType.DragUpdated)
                        && rects[index].Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    }

                    if (Event.current.type == EventType.DragPerform)
                    {
                        for (var i = 0; i < rects.Length; i++)
                        {
                            var rect = rects[i];
                            if (rect.Contains(Event.current.mousePosition) && DragAndDrop.paths != null &&
                                DragAndDrop.paths.Length > 0)
                            {
                                folderPaths[i] = DragAndDrop.paths[0];
                            }
                        }
                    }
                }

                NamingConventionValidatorDatabase.FolderPaths = folderPaths;

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(_running ? "Spinning! " : "Spin It!", GUILayout.Width(500)))
            {
                Run();
            }

            EditorGUILayout.ToggleLeft("Dictionary Loaded", SpellChecker.DictionaryLoaded, GUILayout.Width(150));
            EditorGUILayout.ToggleLeft("Profanity Loaded", SpellChecker.ProfanityLoaded, GUILayout.Width(150));

            EditorGUILayout.LabelField(_running ? "Spinning for (" + stopwatch?.Elapsed.Seconds + ")" : "Idle",
                new GUIStyle()
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal = _running
                        ? new GUIStyleState() {textColor = Color.green}
                        : new GUIStyleState() {textColor = Color.gray}
                }, new[] {GUILayout.ExpandWidth(true)});
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        private void Run()
        {
            _running = true;

            stopwatch = Stopwatch.StartNew();
            checkedGOs = new List<Object>();
            List<Object> allGOs = FindObjectsOfType<GameObject>().Cast<Object>().ToList();

            if (NamingConventionValidatorDatabase.ShouldCheckFolders)
            {
                folderPaths.RemoveAll(x => x == string.Empty);

                if (folderPaths.Count > 0)
                {
                    var itemGUIDs = AssetDatabase.FindAssets("", folderPaths.ToArray());
                    foreach (var itemGUID in itemGUIDs)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(itemGUID);
                        var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

                        allGOs.Add(asset);
                    }
                }
            }

            allGOs = new List<Object>(allGOs.OrderBy(x => x.name));

            checkedGOs = allGOs;

            SpellChecker.Check(allGOs);
            BasicChecker.Check(allGOs);
            CustomChecker.Check(allGOs);

            NamingConventionValidatorResultDisplay.ShowWindow();
            
            _running = false;

            stopwatch.Stop();
        }
        private void OnDestroy()
        {
            NamingConventionValidatorDatabase.SaveStates();
        }
    }
}