using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
#if TMP_PRESENT
using TMPro;
#endif
using UnityEngine;
using UnityEngine.UI;
using WeCantSpell.Hunspell;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    /// <summary>
    /// The class responsible for spell checking
    /// </summary>
    public static class SpellChecker
    {
        //Is dictionary loaded
        public static bool DictionaryLoaded => NamingConventionValidatorDatabase.WordList != null;
        //Is Profanity loaded
        public static bool ProfanityLoaded => NamingConventionValidatorDatabase.ProfanityList.Count > 0;

        ///<value>Results of the check</value>
        public static Dictionary<Object, List<string>> TextFieldResults =
            new Dictionary<Object, List<string>>();

        public static void Check(IEnumerable<Object> objects)
        {
            TextFieldResults = new Dictionary<Object, List<string>>();

            if (NamingConventionValidatorDatabase.TextFieldSpellCheck)
            {
                if (!DictionaryLoaded)
                {
                    try
                    {
                        NamingConventionValidatorDatabase.WordList = WordList.CreateFromFiles(
                            NamingConventionValidatorDatabase.ScriptFolderLocation + @"English (American).dic");
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e + " Failed to load Dictionary, ignoring spelling check");
                        throw;
                    }
                }

                if (NamingConventionValidatorDatabase.TextFieldSpellCheck) TextFieldSpellCheck(objects);
            }

            if (NamingConventionValidatorDatabase.ProfanityCheckTextfield)
            {
                if (!ProfanityLoaded)
                {
                    try
                    {
                        using (StreamReader r =
                            new StreamReader(NamingConventionValidatorDatabase.ScriptFolderLocation + "Profanity.json"))
                        {

                            var json = r.ReadToEnd();
                            NamingConventionValidatorDatabase.ProfanityList =
                                JsonConvert.DeserializeObject<List<string>>(json);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e + " Failed to load Profanity.json, ignoring profanity check");
                        throw;
                    }
                }

                if (NamingConventionValidatorDatabase.ProfanityCheckTextfield) TextFieldProfanityCheck(objects);
            }
        }

        #region textComponentChecks

        //Checking text fields for spelling errors
        private static void TextFieldSpellCheck(IEnumerable<Object> objects)
        {
            var textComponents = new List<Text>();
            #if TMP_PRESENT
            var tmpComponents = new List<TMP_Text>();
            #endif
            
            var gameObjects = objects.ToList();
            gameObjects.RemoveAll(x => !(x is GameObject));
            var castedGo = gameObjects.Cast<GameObject>();
            
            foreach (var obj in castedGo)
            {
                textComponents.AddRange(obj.GetComponents<Text>());
                #if TMP_PRESENT
                tmpComponents.AddRange(obj.GetComponents<TMP_Text>());          
                #endif
            }
            //Text
            foreach (var textComp in textComponents)
            {
                var textToCheck = new string(textComp.text.Where(c => !char.IsPunctuation(c)).ToArray()).Split(' ');

                foreach (var text in textToCheck)
                {
                    var checkDetails = NamingConventionValidatorDatabase.WordList.CheckDetails(text);
                    if (text == string.Empty)
                    {
                        TextFieldResults.Add(textComp.gameObject, new List<string>() {"Empty Field"});
                        break;
                    }

                    if (!checkDetails.Correct)
                    {
                        if (!TextFieldResults.ContainsKey(textComp.gameObject))
                        {
                            TextFieldResults.Add(textComp.gameObject, new List<string>() {text});
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add(text);
                        }
                    }
                }
            }
            //TMP
            #if TMP_PRESENT
            foreach (var textComp in tmpComponents)
            {
                var textToCheck = new string(textComp.text.Where(c => !char.IsPunctuation(c)).ToArray()).Split(' ');

                foreach (var text in textToCheck)
                {
                    var checkDetails = NamingConventionValidatorDatabase.WordList.CheckDetails(text);
                    if (text == string.Empty)
                    {
                        TextFieldResults.Add(textComp.gameObject, new List<string>() {"Empty Field"});
                        break;
                    }

                    if (!checkDetails.Correct)
                    {
                        if (!TextFieldResults.ContainsKey(textComp.gameObject))
                        {
                            TextFieldResults.Add(textComp.gameObject, new List<string>() {text});
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add(text);
                        }
                    }
                }
            }
            #endif

            return;
        }
        
        //Checking text fields for profanity
        private static void TextFieldProfanityCheck(IEnumerable<Object> objects)
        {
            var textComponents = new List<Text>();
            #if TMP_PRESENT
            var tmpComponents = new List<TMP_Text>();
            #endif
            
            var gameObjects = objects.ToList();
            gameObjects.RemoveAll(x => !(x is GameObject));
            var castedGo = gameObjects.Cast<GameObject>();

            foreach (var obj in castedGo)
            {
                textComponents.AddRange(obj.GetComponents<Text>());
                #if TMP_PRESENT
                tmpComponents.AddRange(obj.GetComponents<TMP_Text>());
                #endif
            }
            //Text
            foreach (var textComp in textComponents)
            {
                var textToCheck = new string(textComp.text.Where(c => !char.IsPunctuation(c)).ToArray()).Split(' ');

                foreach (var text in textToCheck)
                {
                    var profanityCheck = NamingConventionValidatorDatabase.ProfanityList.Contains(text.ToLower());

                    if (profanityCheck)
                    {
                        if (!TextFieldResults.ContainsKey(textComp.gameObject))
                        {
                            TextFieldResults.Add(textComp.gameObject, new List<string>() {$"Profanity: {text}"});
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add($"Profanity: {text}");
                        }
                    }
                }
            }
            //TMP
            #if TMP_PRESENT
            foreach (var textComp in tmpComponents)
            {
                var textToCheck = new string(textComp.text.Where(c => !char.IsPunctuation(c)).ToArray()).Split(' ');

                foreach (var text in textToCheck)
                {
                    var profanityCheck = NamingConventionValidatorDatabase.ProfanityList.Contains(text.ToLower());

                    if (profanityCheck)
                    {
                        if (!TextFieldResults.ContainsKey(textComp.gameObject))
                        {
                            TextFieldResults.Add(textComp.gameObject, new List<string>() {$"Profanity: {text}"});
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add($"Profanity: {text}");
                        }
                    }
                }
            }
            #endif
        }

        #endregion
    }
}