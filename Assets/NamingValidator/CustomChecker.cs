using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    public static class CustomChecker
    {
        public static IssueData CustomCheckerResults = new IssueData();

        public static void Check(IEnumerable<Object> objects)
        {
            CustomCheckerResults = new IssueData();
            var gameObjects = objects as Object[] ?? objects.ToArray();
            if (!NamingConventionValidatorDatabase.CustomValidatorsEnabled || !gameObjects.Any() ||
                NamingConventionValidatorDatabase.CustomNamingValidators.Count == 0) return;
            
            foreach (var obj in gameObjects)
            {
                foreach (var namingValidator in NamingConventionValidatorDatabase.CustomNamingValidators)
                {
                    try
                    {
                        namingValidator.Evaluate(obj, CustomCheckerResults);
                    }
                    catch (NullReferenceException e)
                    {
                       Debug.Log(namingValidator.name + " " + e);
                    }
                }
                
                NamingConventionValidator.NeedCustomValidatorRedraw = true;
            }
        }
    }
}