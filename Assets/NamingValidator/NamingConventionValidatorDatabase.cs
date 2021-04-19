using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WeCantSpell.Hunspell;

namespace NamingValidator
{
    public static class NamingConventionValidatorDatabase
    {
        public static WordList WordList;
        public static List<string> ProfanityList = new List<string>();
        
        public static List<string> FolderPaths = new List<string>();
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
        public static bool TextFieldSpellCheck{
            get => EditorPrefs.HasKey("TextFieldSpellCheck") && EditorPrefs.GetBool("TextFieldSpellCheck");
            set => EditorPrefs.SetBool("TextFieldSpellCheck", value);
        }
        public static bool ProfanityCheckTextfield{
            get => EditorPrefs.HasKey("TextFieldProfanityCheck") && EditorPrefs.GetBool("TextFieldProfanityCheck");
            set => EditorPrefs.SetBool("TextFieldProfanityCheck", value);
        }
        public static bool CheckForParenthesis {
            get => EditorPrefs.HasKey("CheckForParenthesis") && EditorPrefs.GetBool("CheckForParenthesis");
            set => EditorPrefs.SetBool("CheckForParenthesis", value);
        }
        public static bool CheckForDefaultNames
        {
            get => EditorPrefs.HasKey("CheckForDefaultNames") && EditorPrefs.GetBool("CheckForDefaultNames");
            set => EditorPrefs.SetBool("CheckForDefaultNames", value);
        }
        public enum SpacingConvention
        {
            Allow,
            Disallow,
            AllowOnlyOnTopObjects
        }

        public static SpacingConvention SpacingConv { 
            get => EditorPrefs.HasKey("SpacingConvention") ? (SpacingConvention)Enum.Parse(typeof(SpacingConvention), EditorPrefs.GetString("SpacingConvention")) : SpacingConvention.Allow; 
            set => EditorPrefs.SetString("SpacingConvention", value.ToString());
        }

        public enum CapitalizationConvention
        {
            None,
            CamelCase,
            PascalCase
        }

        public static CapitalizationConvention CapitalizationConv{ 
            get => EditorPrefs.HasKey("CapitalizationConvention") ? (CapitalizationConvention)Enum.Parse(typeof(CapitalizationConvention), EditorPrefs.GetString("CapitalizationConvention")) : CapitalizationConvention.None; 
            set => EditorPrefs.SetString("CapitalizationConvention", value.ToString());
        }
        public static string FolderLocation { get; set; }
    }
}