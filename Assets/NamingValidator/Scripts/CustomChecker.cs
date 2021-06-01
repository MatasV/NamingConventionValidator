using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    /// <summary>
    /// The class responsible for enumerating on all the custom validators and outputting the collective issues
    /// </summary>
    public static class CustomChecker
    {
        ///<value>Results of the check</value>
        public static IssueData CustomCheckerResults = new IssueData();

        public static void Check(IEnumerable<Object> objects)
        {
            CustomCheckerResults = new IssueData();
            var gameObjects = objects as Object[] ?? objects.ToArray();
            if (!NamingConventionValidatorDatabase.CustomValidatorsEnabled || !gameObjects.Any() ||
                NamingConventionValidatorDatabase.CustomNamingValidators.Count == 0) return;
            
            //enumerate on the object list
            foreach (var obj in gameObjects)
            {
                //enumerate on the validator list
                foreach (var namingValidator in NamingConventionValidatorDatabase.CustomNamingValidators)
                {
                    try
                    {
                        //Call the Evaluate method on the custom validator
                        namingValidator.Evaluate(obj, CustomCheckerResults);
                    }
                    catch (NullReferenceException)
                    {
                       Debug.Log("Null Reference in Custom Validator list, please check!");
                    }
                }
            }
        }
    }
}