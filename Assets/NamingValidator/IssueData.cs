using System.Collections.Generic;
using UnityEngine;

namespace NamingValidator
{
    public class IssueData
    {
        public void AddIssue(GameObject obj, string issue)
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

        public Dictionary<GameObject, List<string>> GetIssueData { get; } = new Dictionary<GameObject, List<string>>();
    }
}