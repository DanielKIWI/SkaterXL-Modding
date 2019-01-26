using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using System;

namespace XLShredTrickNameGUI
{
    public static class Main {
        public static bool enabled;
        public static String modId;
        private static HarmonyInstance harmonyInstance;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            modId = modEntry.Info.Id;
            harmonyInstance = HarmonyInstance.Create(modId);
            modEntry.OnToggle = OnToggle;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                //harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                new GameObject().AddComponent<TrickNameGUI>();
            } else if (TrickNameGUI.Instance != null) {
                //harmonyInstance.UnpatchAll(harmonyInstance.Id);
                GameObject.Destroy(TrickNameGUI.Instance.gameObject);
            }
            return true;
        }
    }

}
