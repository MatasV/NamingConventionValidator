using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using ReorderableList = UnityEditorInternal.ReorderableList;

namespace NamingValidator
{
    /// <summary>
    /// The main window class
    /// </summary>
    public sealed class NamingConventionValidator : EditorWindow
    {
        private bool running = false;

        public List<CustomNamingValidator> customNamingValidators;
        private List<string> folderPaths;
        private Rect[] rects;

        public static List<Object> CheckedGOs;
        
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

        //Drawing
        private void OnGUI()
        {
            //Displaying Basic Options
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

            //Displaying Spell Check Options
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

            //Displaying Custom Options
            NamingConventionValidatorDatabase.CustomValidatorsEnabled = EditorGUILayout.BeginToggleGroup(new GUIContent(
                "Custom Validators Enabled",
                "Enable custom validators"), NamingConventionValidatorDatabase.CustomValidatorsEnabled);
            //Displaying the Validator List
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
            //Displaying Folders
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
                    rects[index] = EditorGUILayout.GetControlRect();
                    folderPaths[index] = EditorGUI.TextField(rects[index], folderPaths[index]);
                    
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
            //Run Button
            if (GUILayout.Button(running ? "Spinning! " : "Spin It!"))
            {
                Run();
            }
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.ToggleLeft("Dictionary Loaded", SpellChecker.DictionaryLoaded);
            EditorGUILayout.ToggleLeft("Profanity Loaded", SpellChecker.ProfanityLoaded);
            
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        /// <summary>
        /// Starting the checking process
        /// </summary>
        private void Run()
        {
            running = true;
            
            //Get all objects from the current scene
            CheckedGOs = new List<Object>();
            List<Object> allGOs = FindObjectsOfType<GameObject>().Cast<Object>().ToList();

            //Get all objects from the specified folders
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

            //Ordering objects by name
            allGOs = new List<Object>(allGOs.OrderBy(x => x.name));

            CheckedGOs = allGOs;

            //Running the checkers
            SpellChecker.Check(allGOs);
            BasicChecker.Check(allGOs);
            CustomChecker.Check(allGOs);

            //Opening the display window
            NamingConventionValidatorResultDisplay.ShowWindow();
            
            running = false;
            
        }
        private void OnDestroy()
        {
            NamingConventionValidatorDatabase.SaveStates();
        }
    }
}