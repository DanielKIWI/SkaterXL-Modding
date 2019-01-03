using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using System;

namespace XLShredReplayEditor {

    [Serializable]
    public class Settings : UnityModManager.ModSettings {

        public bool adjustAudioPitch = true;

        public override void Save(UnityModManager.ModEntry modEntry) {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    static class Main {

        public static bool enabled;
        public static Settings settings;
        public static String modId;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            try {
                settings = Settings.Load<Settings>(modEntry);
                modId = modEntry.Info.Id;
                modEntry.OnSaveGUI = OnSaveGUI;
                modEntry.OnToggle = OnToggle;
            } catch (Exception e) {
                DebugGUI.LogException(e);
            }
            GameObject rmGO = new GameObject("ReplayEditor");
            ReplayManager rm = rmGO.AddComponent<ReplayManager>();
            PromptController.Instance.menuthing.enabled = false;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            DebugGUI.Log("Changed ReplayMod enabled to " + value);
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            settings.Save(modEntry);
        }
    }
}
