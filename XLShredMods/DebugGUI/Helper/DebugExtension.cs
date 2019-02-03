using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DebugGUI.Helper {
    public static class DebugHelper {
        private static string indentString = " | ";
        public static void LogObjectHierarchy(GameObject go) {
            Debug.Log(GetObjectHierarchyDescription(go, 0));
        }
        public static string GetObjectHierarchyDescription(GameObject gameObject, int indentLevel) {
            string result = "";
            for (int i = 0; i < indentLevel; i++) {
                result += indentString;
            }
            result += gameObject.name + " (Components: [";
            List<string> componentDescriptions = new List<string>();
            foreach (var c in gameObject.GetComponents<Component>()) {
                if (c is Transform) continue;
                componentDescriptions.Add(c.GetType().Name);
            }
            result += string.Join(", ", componentDescriptions.ToArray());
            result += "]";
            if (gameObject.transform.childCount > 0) {
                List<string> childDescriptions = new List<string>();
                for (int i = 0; i < gameObject.transform.childCount; i++) {
                    childDescriptions.Add(GetObjectHierarchyDescription(gameObject.transform.GetChild(i).gameObject, indentLevel + 1));
                }
                result += "\n" + string.Join("\n", childDescriptions.ToArray());
            }
            return result;
        }
    }
}
