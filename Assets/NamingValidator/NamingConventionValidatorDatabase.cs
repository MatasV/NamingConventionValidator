using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using WeCantSpell.Hunspell;

namespace NamingValidator
{
    /// <summary>
    /// The class containing the data related to the Naming Convention Validator
    /// </summary>
    public static class NamingConventionValidatorDatabase
    {
        /// <value>The current world list used in spell checking</value>
        public static WordList WordList;
        /// <value>The current profanity list used in spell checking</value>
        public static List<string> ProfanityList = new List<string>();
        
        private static List<string> _folderPaths = new List<string>();
        /// <value>Are the folder paths initialized</value>
        private static bool _folderPathsInit = false;

        /// <value> Is TextMeshPro present?</value>
        public static bool IsTMPPresent = false;
        
        /// <value>Are the custom validators initialized</value>
        private static bool _customValidatorsInit = false;
        private static List<CustomNamingValidator> _customNamingValidators = new List<CustomNamingValidator>();

        /// <value>The custom validator list ist used by <see cref="CustomChecker"/></value>
        public static List<CustomNamingValidator> CustomNamingValidators
        {
            get
            {
                if (!_customValidatorsInit)
                {
                    _customNamingValidators = new List<CustomNamingValidator>();
                    if (File.Exists(FolderLocation + "CustomValidatorPaths.json"))
                    {
                        using (StreamReader r =
                            new StreamReader(FolderLocation + "CustomValidatorPaths.json"))
                        {
                            var json = r.ReadToEnd();
                            var paths = JsonConvert.DeserializeObject<List<string>>(json);
                            foreach (var path in paths)
                            {
                                var asset = AssetDatabase.LoadAssetAtPath<CustomNamingValidator>(path);
                                if (asset != null)
                                {
                                    _customNamingValidators.Add(asset);
                                }
                                else
                                {
                                    Debug.LogWarning("Custom Naming Validator at path: " + path +
                                                     " could not be loaded, check if it still exists");
                                }
                            }
                        }

                        _customValidatorsInit = true;
                        return _customNamingValidators;
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(_customNamingValidators);
                        using (StreamWriter w = new StreamWriter(FolderLocation + "CustomValidatorPaths.json"))
                        {
                            w.Write(json);
                        }
                        _customValidatorsInit = true;
                        return _customNamingValidators;
                    }
                }
                
                return _customNamingValidators;
            }
            set => _customNamingValidators = value;
        }
        /// <value>The folder path list used in asset name checking</value>
        public static List<string> FolderPaths
        {
            get
            {
                if (!_folderPathsInit)
                {
                    _folderPaths = new List<string>();
                    
                    if (File.Exists(FolderLocation + "FolderPaths.json"))
                    {
                        using (StreamReader r =
                            new StreamReader(FolderLocation + "FolderPaths.json"))
                        {
                            var json = r.ReadToEnd();
                            _folderPaths = JsonConvert.DeserializeObject<List<string>>(json);
                        }
                       
                        _folderPathsInit = true;
                        return _folderPaths;
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(_folderPaths);
                        using (StreamWriter w = new StreamWriter(FolderLocation + "FolderPaths.json"))
                        { 
                            w.Write(json);
                        }
                        
                        _folderPathsInit = true;
                        return _folderPaths;
                    }
                }

                return _folderPaths;
            }
            set => _folderPaths = value;
        }

        /// <summary>
        /// Method that saves the current Validators and folder paths
        /// </summary>
        public static void SaveStates()
        {
            //saving validator paths
            var assetLocations = (from customNamingValidator in _customNamingValidators where 
                customNamingValidator != null select AssetDatabase.GetAssetPath(customNamingValidator)).ToList();

            var valJson = JsonConvert.SerializeObject(assetLocations);
            using (StreamWriter valW = new StreamWriter(FolderLocation + "CustomValidatorPaths.json"))
            {
                valW.Write(valJson);
            }
            
            //Saving folder paths
            var foldJson = JsonConvert.SerializeObject(_folderPaths);
            using  (StreamWriter foldW = new StreamWriter(FolderLocation + "FolderPaths.json"))
            {
                foldW.Write(foldJson);
            }
        }
        
        #region booleans
        public static bool ShouldCheckFolders
        {
            get => EditorPrefs.HasKey("ShouldCheckFolders") && EditorPrefs.GetBool("ShouldCheckFolders");
            set => EditorPrefs.SetBool("ShouldCheckFolders", value);
        }

        public static bool CustomValidatorsEnabled
        {
            get => EditorPrefs.HasKey("CustomValidatorsEnabled") && EditorPrefs.GetBool("CustomValidatorsEnabled");
            set => EditorPrefs.SetBool("CustomValidatorsEnabled", value);
        }

        public static bool TextFieldSpellCheck
        {
            get => EditorPrefs.HasKey("TextFieldSpellCheck") && EditorPrefs.GetBool("TextFieldSpellCheck");
            set => EditorPrefs.SetBool("TextFieldSpellCheck", value);
        }

        public static bool ProfanityCheckTextfield
        {
            get => EditorPrefs.HasKey("TextFieldProfanityCheck") && EditorPrefs.GetBool("TextFieldProfanityCheck");
            set => EditorPrefs.SetBool("TextFieldProfanityCheck", value);
        }

        public static bool CheckForParenthesis
        {
            get => EditorPrefs.HasKey("CheckForParenthesis") && EditorPrefs.GetBool("CheckForParenthesis");
            set => EditorPrefs.SetBool("CheckForParenthesis", value);
        }

        public static bool CheckForDefaultNames
        {
            get => EditorPrefs.HasKey("CheckForDefaultNames") && EditorPrefs.GetBool("CheckForDefaultNames");
            set => EditorPrefs.SetBool("CheckForDefaultNames", value);
        }

        #endregion
        
        public enum SpacingConvention
        {
            Allow, //Allow spaces
            Disallow, //Disallow spaces
            AllowOnlyOnTopObjects //Only allow spaces on top objects
        }
        
        //Current spacing convention
        public static SpacingConvention SpacingConv
        {
            get => EditorPrefs.HasKey("SpacingConvention")
                ? (SpacingConvention) Enum.Parse(typeof(SpacingConvention), EditorPrefs.GetString("SpacingConvention"))
                : SpacingConvention.Allow;
            set => EditorPrefs.SetString("SpacingConvention", value.ToString());
        }

        public enum CapitalizationConvention
        {
            None, //Dont check
            CamelCase, //Enforce camel case
            PascalCase //Enforce pascal case
        }

        //Current capitalization convention
        public static CapitalizationConvention CapitalizationConv
        {
            get => EditorPrefs.HasKey("CapitalizationConvention")
                ? (CapitalizationConvention) Enum.Parse(typeof(CapitalizationConvention),
                    EditorPrefs.GetString("CapitalizationConvention"))
                : CapitalizationConvention.None;
            set => EditorPrefs.SetString("CapitalizationConvention", value.ToString());
        }

        private static string _folderLocation = string.Empty;
        
        ///<value> Where are the scripts located? Used in accessing the <i>Dictionary</i>, <i>Profanity list</i>, <i>Saved folder list</i>, <i>Default name list</i></value>
        public static string FolderLocation
        {
            get
            {
                if (_folderLocation == string.Empty)
                {
                    var res = Directory.GetFiles(Application.dataPath, "NamingConventionValidator.cs",
                        SearchOption.AllDirectories);
                    if (res.Length == 0)
                    {
                        Debug.LogWarning("Current folder not found! Features will not work properly.");
                    }

                    _folderLocation = res[0].Replace("NamingConventionValidator.cs", "").Replace("\\", "/");
                }
                return _folderLocation;
            }
        }
    }
}