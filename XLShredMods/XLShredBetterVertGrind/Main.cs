using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using System;

namespace XLShredBetterVertGrind {
    public static class Main {
        public static bool enabled;
        public static String modId;
        private static HarmonyInstance harmonyInstance;
        private static bool isPatched;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            modId = modEntry.Info.Id;
            harmonyInstance = HarmonyInstance.Create(modId);
            modEntry.OnToggle = OnToggle;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                if (!isPatched) {
                    harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                    Debug.Log("Patched Assembly for Better Vert Grind");
                    isPatched = true;
                }
            } else if (isPatched) {
                harmonyInstance.UnpatchAll(harmonyInstance.Id);
                Debug.Log("Unpatched Assembly for Better Vert Grind");
                isPatched = false;
            }
            return true;
        }
    }
}
