using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    //Class that validates objects based on their position in the hierarchy.
    [CreateAssetMenu(menuName = "Custom Name Validators/TopLevelPrefixPostfixValidator", fileName = "TopLevelPrefixPostfixValidator")]
    public class TopLevelPrefixPostfixValidator : CustomNamingValidator
    {
        //required prefix and postfix
        public string prefix;
        public string postfix;
        public override void Evaluate(Object obj, IssueData issueData)
        {
            try
            {
                if (obj is GameObject gameObject)
                {
                    //Checking if the objects has a parent
                    if (gameObject.transform.parent == null)
                    {
                        //Verifying the naming convention, if it fails, add it to the issue list.
                        var objName = obj.name;
                        if (prefix != string.Empty)
                        {
                            if (!objName.StartsWith(prefix)) issueData.AddIssue(gameObject, "Missing Prefix");
                        }

                        if (postfix != string.Empty)
                        {
                            if (!objName.EndsWith(postfix)) issueData.AddIssue(gameObject, "Missing Postfix");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
         
        }
    }
}