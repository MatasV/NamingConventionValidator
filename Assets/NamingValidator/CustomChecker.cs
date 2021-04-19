using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NamingValidator
{
    public static class CustomChecker
    {
        public static IssueData CustomCheckerResults = new IssueData();
        public static List<CustomNamingValidator> CustomNamingValidators = new List<CustomNamingValidator>();
        public static void Check(IEnumerable<Object> objects)
        {
            CustomCheckerResults = new IssueData();
            var gameObjects = objects as Object[] ?? objects.ToArray();
            if (!NamingConventionValidatorDatabase.CustomValidatorsEnabled || !gameObjects.Any() ||
                CustomNamingValidators.Count == 0) return;
            
            foreach (var obj in gameObjects)
            {
                foreach (var namingValidator in CustomNamingValidators)
                {
                    namingValidator.Evaluate(obj, CustomCheckerResults);
                }
                NamingConventionValidator.NeedCustomValidatorRedraw = true;
            }
        }
    }
}
