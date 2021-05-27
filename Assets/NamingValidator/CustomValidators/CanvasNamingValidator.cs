using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    //Class that validates objects based on their components.
    [CreateAssetMenu(menuName = "Custom Name Validators/CanvasNamingValidator", fileName = "CanvasNamingValidator")]
    public class CanvasNamingValidator : CustomNamingValidator
    {
        //The string that is required to be present if an object has the Canvas component
        public string requiredString;
        public override void Evaluate(Object obj, IssueData issueData)
        {
            try
            {
                if (obj is GameObject gameObject)
                {
                    //if GameObject has a Canvas component
                    if (gameObject.GetComponents(typeof(Component)).Any(x => x is Canvas))
                    {
                        var objName = obj.name;
                        if (requiredString != string.Empty)
                        {
                            //if GameObject does not have a Canvas component, add it to the issue data, otherwise, do nothing
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