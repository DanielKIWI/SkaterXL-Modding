using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using System;

namespace DebugGUI {

    [Serializable]
    public class Settings : UnityModManager.ModSettings {

        public int MaxLogsCount = 20;
        public float MessageLifeTime = 10f;

        public override void Save(UnityModManager.ModEntry modEntry) {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    static class Main {
        public static bool enabled;
        public static Settings settings;
        public static String modId;
        private static ILogHandler unityLogHandler;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            settings = Settings.Load<Settings>(modEntry);

            modId = modEntry.Info.Id;

            unityLogHandler = Debug.unityLogger.logHandler;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                DebugGUI.Instance.enabled = true;
                DebugGUI.Instance.parentHandler = unityLogHandler;
                Debug.unityLogger.logHandler = DebugGUI.Instance;
            } else {
                Debug.unityLogger.logHandler = unityLogHandler;
                DebugGUI.Instance.parentHandler = null;
                GameObject.Destroy(DebugGUI.Instance.gameObject);
            }
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            settings.Save(modEntry);
        }
    }
}
