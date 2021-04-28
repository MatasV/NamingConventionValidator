using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeCantSpell.Hunspell;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    public static class SpellChecker
    {
        public static bool DictionaryLoaded => NamingConventionValidatorDatabase.WordList != null;
        public static bool ProfanityLoaded => NamingConventionValidatorDatabase.ProfanityList.Count > 0;

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
                            NamingConventionValidatorDatabase.FolderLocation + @"English (American).dic");
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
                        using StreamReader r =
                            new StreamReader(NamingConventionValidatorDatabase.FolderLocation + "Profanity.json");
                        var json = r.ReadToEnd();
                        NamingConventionValidatorDatabase.ProfanityList =
                            JsonConvert.DeserializeObject<List<string>>(json);
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

        private static void TextFieldSpellCheck(IEnumerable<Object> objects)
        {
            var textComponents = new List<Text>();
            var tmpComponents = new List<TMP_Text>();

            var gameObjects = objects.ToList();
            gameObjects.RemoveAll(x => !(x is GameObject));
            var castedGo = gameObjects.Cast<GameObject>();
            
            foreach (var obj in castedGo)
            {
                textComponents.AddRange(obj.GetComponents<Text>());
                tmpComponents.AddRange(obj.GetComponents<TMP_Text>());
            }

            foreach (var textComp in textComponents)
            {
                var textToCheck = new string(textComp.text.Where(c => !char.IsPunctuation(c)).ToArray()).Split(' ');

                foreach (var text in textToCheck)
                {
                    var checkDetails = NamingConventionValidatorDatabase.WordList.CheckDetails(text);
                    if (text == string.Empty)
                    {
                        TextFieldResults.Add(textComp.gameObject, new List<string>() {"Empty Field"});
                        NamingConventionValidator.NeedSpellCheckRedraw = true;
                        break;
                    }

                    if (!checkDetails.Correct)
                    {
                        if (!TextFieldResults.ContainsKey(textComp.gameObject))
                        {
                            TextFieldResults.Add(textComp.gameObject, new List<string>() {text});
                            NamingConventionValidator.NeedSpellCheckRedraw = true;
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add(text);
                        }
                    }
                }
            }

            foreach (var textComp in tmpComponents)
            {
                var textToCheck = new string(textComp.text.Where(c => !char.IsPunctuation(c)).ToArray()).Split(' ');

                foreach (var text in textToCheck)
                {
                    var checkDetails = NamingConventionValidatorDatabase.WordList.CheckDetails(text);
                    if (text == string.Empty)
                    {
                        TextFieldResults.Add(textComp.gameObject, new List<string>() {"Empty Field"});
                        NamingConventionValidator.NeedSpellCheckRedraw = true;
                        break;
                    }

                    if (!checkDetails.Correct)
                    {
                        if (!TextFieldResults.ContainsKey(textComp.gameObject))
                        {
                            TextFieldResults.Add(textComp.gameObject, new List<string>() {text});
                            NamingConventionValidator.NeedSpellCheckRedraw = true;
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add(text);
                        }
                    }
                }
            }

            return;
        }

        private static void TextFieldProfanityCheck(IEnumerable<Object> objects)
        {
            var textComponents = new List<Text>();
            var tmpComponents = new List<TMP_Text>();

            var gameObjects = objects.ToList();
            gameObjects.RemoveAll(x => !(x is GameObject));
            var castedGo = gameObjects.Cast<GameObject>();

            foreach (var obj in castedGo)
            {
                textComponents.AddRange(obj.GetComponents<Text>());
                tmpComponents.AddRange(obj.GetComponents<TMP_Text>());
            }

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
                            NamingConventionValidator.NeedSpellCheckRedraw = true;
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add($"Profanity: {text}");
                        }
                    }
                }
            }

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
                            NamingConventionValidator.NeedSpellCheckRedraw = true;
                        }
                        else
                        {
                            TextFieldResults[textComp.gameObject].Add($"Profanity: {text}");
                        }
                    }
                }
            }
        }

        #endregion
    }
}