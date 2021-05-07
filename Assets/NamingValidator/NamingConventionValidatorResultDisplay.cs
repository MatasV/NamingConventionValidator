using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NamingValidator
{
    public sealed class NamingConventionValidatorResultDisplay : EditorWindow
    {
        Vector2 scrollPos;

        private static NamingConventionValidatorResultDisplay window;
        
        public static void ShowWindow()
        {
            window = GetWindow<NamingConventionValidatorResultDisplay>();
            window.titleContent = new GUIContent("Naming Results");
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPos =
                EditorGUILayout.BeginScrollView(scrollPos);
            
            if (NamingConventionValidator.checkedGOs != null && NamingConventionValidator.checkedGOs.Count > 0)
            {
                foreach (var obj in NamingConventionValidator.checkedGOs)
                {
                    if (ResultsContainObject(obj))
                    {
                        EditorGUILayout.BeginHorizontal();
                        var objField = EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                        
                        if (GUILayout.Button("Issues"))
                        {
                            var issueDisplay = ScriptableObject.CreateInstance<NamingConventionValidatorObjectResults>();
                            issueDisplay.ShowWindow(obj);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                }
            }
            else
            {
                EditorGUILayout.LabelField("Run the check!" );
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool ResultsContainObject(Object obj)
        {
            return (SpellChecker.TextFieldResults.ContainsKey(obj) || BasicChecker.BasicCheckResults.ContainsKey(obj) ||
                    CustomChecker.CustomCheckerResults.GetIssueData.ContainsKey(obj));
        }
    }

    public sealed class NamingConventionValidatorObjectResults : EditorWindow
    {
        [SerializeField] private Object displayObj;
        
        [SerializeField] TreeViewState treeViewState;
        
        IssueTreeView issueTreeView;
        public void ShowWindow(Object obj)
        {
            var window = GetWindow<NamingConventionValidatorObjectResults>();
            displayObj = obj;
            if (treeViewState == null)
                treeViewState = new TreeViewState ();
            
            issueTreeView = new IssueTreeView(treeViewState, displayObj);
             
            window.titleContent = new GUIContent(obj.name + " Issues");
            window.Show();
        }

        void OnGUI()
        {
            if(issueTreeView != null) issueTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
            else this.Close();
        }
    }
    
    public class IssueTreeView : TreeView
        {
            public IssueTreeView(TreeViewState treeViewState, Object obj)
                : base(treeViewState)
            {
                this.obj = obj;
                Reload();
            }

            private Object obj;
            protected override TreeViewItem BuildRoot ()
            {
                var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};

                int issueID = 5;
                
                if (BasicChecker.BasicCheckResults.ContainsKey(obj))
                {
                    var basicCheck = new TreeViewItem {id = 1, depth = 0, displayName = "Basic Check Results"};
                    root.AddChild(basicCheck);
                    
                    foreach (var issue in BasicChecker.BasicCheckResults[obj])
                    {
                        var item = new TreeViewItem {id = issueID, depth = 1, displayName = issue};
                        basicCheck.AddChild(item);
                        issueID++;
                    }
                }

                
                if (SpellChecker.TextFieldResults.ContainsKey(obj))
                {
                    var spellCheck = new TreeViewItem {id = 2, depth = 0, displayName = "Spell Check Results"};
                    
                    root.AddChild(spellCheck);
                    
                    foreach (var issue in SpellChecker.TextFieldResults[obj])
                    {
                        var item = new TreeViewItem {id = issueID, depth = 1, displayName = issue};
                        spellCheck.AddChild(item);
                        issueID++;
                    }
                }

                if (CustomChecker.CustomCheckerResults.GetIssueData.ContainsKey(obj))
                {
                    var customCheck = new TreeViewItem {id = 3, depth = 0, displayName = "Custom Check Results"};
                    root.AddChild(customCheck);
                    
                    foreach (var issue in CustomChecker.CustomCheckerResults.GetIssueData[obj])
                    {
                        var item = new TreeViewItem {id = issueID, depth = 1, displayName = issue};
                        customCheck.AddChild(item);
                        issueID++;
                    }
                    
                }
                
                return root;
            }
        }
}