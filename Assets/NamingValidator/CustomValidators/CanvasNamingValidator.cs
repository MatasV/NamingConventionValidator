using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    [CreateAssetMenu(menuName = "Custom Name Validators/CanvasNamingValidator", fileName = "CanvasNamingValidator")]
    public class CanvasNamingValidator : CustomNamingValidator
    {
        public string requiredString;
        public override void Evaluate(Object obj, IssueData issueData)
        {
            try
            {
                if (obj is GameObject gameObject)
                {
                    if (gameObject.GetComponents(typeof(Component)).Any(x => x is Canvas))
                    {
                        var objName = obj.name;
                        if (requiredString != string.Empty)
                        {
                            if (!objName.Contains(requiredString)) issueData.AddIssue(gameObject, "Canvas name is missing required string: " + requiredString);
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