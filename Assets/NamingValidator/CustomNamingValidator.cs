using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    public abstract class CustomNamingValidator : ScriptableObject
    {
        public virtual void Evaluate(Object obj, IssueData issueData)
        {
            try
            {
               Debug.Log($"Default Evaluation: {obj.name}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
           
        }
    }
}