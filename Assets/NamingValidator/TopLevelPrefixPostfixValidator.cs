using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    [CreateAssetMenu(menuName = "Custom Name Validators/TopLevelPrefixPostfixValidator", fileName = "TopLevelPrefixPostfixValidator")]
    public class TopLevelPrefixPostfixValidator : CustomNamingValidator
    {
        public string prefix;
        public string postfix;
        public override void Evaluate(Object obj, IssueData issueData)
        {
            try
            {
                if (obj is GameObject gameObject)
                {
                    if (gameObject.transform.parent == null)
                    {
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