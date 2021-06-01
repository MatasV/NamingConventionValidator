using System;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamingValidator
{
    /// <summary>
    /// The skeleton class that is used by <see cref="CustomChecker"/> to evaluate issues.
    /// </summary>
    [Serializable]
    public abstract class CustomNamingValidator : ScriptableObject
    {
        /// <summary>
        /// The method to override in order to evaluate issues.
        ///  <param name="obj">An object to be checked.</param>
        /// <param name="issueData">Issue data for the current session.</param>
        /// </summary>
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