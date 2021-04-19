using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    public static class BasicChecker
    {
        public static List<string> DefaultNames = new List<string>();

        public static Dictionary<Object, List<string>> BasicCheckResults =
            new Dictionary<Object, List<string>>();

        private const string ParenthesisPattern = @"\([0-9][0-9]?\)";
        
        public static void Check(IEnumerable<Object> objects)
        {
            BasicCheckResults = new Dictionary<Object, List<string>>();

            foreach (var obj in objects)
            {
                var objectName = obj.name;
                var foundIssues = new List<string>();

                if (CheckParenthesis(objectName, foundIssues) | CheckForDefaultNaming(objectName, foundIssues) |
                    CheckForSpacing(obj, foundIssues) | CheckForCapitalisationConvention(obj, foundIssues))
                {
                    NamingConventionValidator.NeedBasicRedraw = true;
                    BasicCheckResults.Add(obj, foundIssues);
                }
            }
        }

        private static bool CheckParenthesis(string name, List<string> issues)
        {
            if (!NamingConventionValidatorDatabase.CheckForParenthesis)
            {
                return false;
            }
            if (Regex.IsMatch(name, ParenthesisPattern))
            {
                issues.Add("Illegal Parenthesis");
                return true;
            }
            return false;
        }

        private static bool CheckForDefaultNaming(string name, List<string> issues)
        {
            if (!NamingConventionValidatorDatabase.CheckForDefaultNames) return false;
            try
            {
                using StreamReader sr =
                    new StreamReader(NamingConventionValidatorDatabase.FolderLocation + @"/DefaultNames.txt");
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Debug.Log(line);
                    var pattern = $@"(^{line}$)|(^{line} \([0-9][0-9]?\))";
                    if (Regex.IsMatch(name, pattern))
                    {
                        
                        issues.Add("Default Name");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e + ", ignoring default naming checking");
                return false;
            }
            return false;
        }

        private static bool CheckForSpacing(Object obj, List<string> issues)
        {
            switch (NamingConventionValidatorDatabase.SpacingConv)
            {
                case NamingConventionValidatorDatabase.SpacingConvention.Allow:
                    return false;
                case NamingConventionValidatorDatabase.SpacingConvention.Disallow:
                    if (Regex.IsMatch(obj.name, " "))
                    {
                        issues.Add("Spacing");
                        return true;
                    }
                    return false;
                case NamingConventionValidatorDatabase.SpacingConvention.AllowOnlyOnTopObjects:
                    if (obj is GameObject gameObject)
                    {
                        if (gameObject.transform.parent != null)
                        {
                            if (Regex.IsMatch(gameObject.name, " "))
                            {
                                issues.Add("Non-Parent Spacing");
                                return true;
                            }
                        }
                    }

                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool CheckForCapitalisationConvention(Object obj, List<string> issues)
        {
            var words = obj.name.Split(' ');
            switch (NamingConventionValidatorDatabase.CapitalizationConv)
            {
                case NamingConventionValidatorDatabase.CapitalizationConvention.None:
                    return false;
                case NamingConventionValidatorDatabase.CapitalizationConvention.CamelCase:
                    
                    if (words.Any(word => !char.IsLower(word[0]) && char.IsLetter(word[0])))
                    {
                        issues.Add("Wrong Capitalization");
                        return true;
                    }
                    return false;
                case NamingConventionValidatorDatabase.CapitalizationConvention.PascalCase:
                    if (words.Any(word => !char.IsUpper(word[0]) && char.IsLetter(word[0])))
                    {
                        issues.Add("Wrong Capitalization");
                        return true;
                    }
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}