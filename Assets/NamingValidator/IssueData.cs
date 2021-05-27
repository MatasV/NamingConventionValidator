using System.Collections.Generic;
using UnityEngine;

namespace NamingValidator
{
    /// <summary>
    /// The class holding all the issue information for custom validators.
    /// </summary>
    public class IssueData
    {
        /// <summary>
        /// The method for adding a new <b>issue</b> to the given <b>object</b>.
        /// </summary>
        /// <param name="obj">An object to be checked.</param>
        /// <param name="issue">The issue related to the object.</param>
        public void AddIssue(Object obj, string issue)
        {
            if (obj == null || issue == string.Empty)
            {
                Debug.Log("Bad issue message, ignoring");
                return;
            }

            if (GetIssueData.ContainsKey(obj))
            {
                GetIssueData[obj].Add(issue);
            }
            else
            {
                GetIssueData.Add(obj, new List<string>() {issue});
            }
        }
        
        /// <summary>
        /// The property used to get the current issue data.
        /// </summary>
        public Dictionary<Object, List<string>> GetIssueData { get; } = new Dictionary<Object, List<string>>();
    }
}