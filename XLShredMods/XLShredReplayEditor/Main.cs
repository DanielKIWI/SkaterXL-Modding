using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using System;
#if !STANDALONE
using XLShredLib;
using XLShredLib.UI;
#endif

namespace XLShredReplayEditor {

    [Serializable]
    public class Settings : UnityModManager.ModSettings {
        
        public float MaxRecordedTime = 120f;
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
            settings = Settings.Load<Settings>(modEntry);
            modId = modEntry.Info.Id;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnSettingsGUI;
            ReplayManager rm = new GameObject("ReplayEditor").AddComponent<ReplayManager>();
            //Disabling the Tutorial
            PromptController.Instance.menuthing.enabled = false;
#if !STANDALONE
            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("kiwi", "Kiwi");
            uiBoxKiwi.AddLabel("Start-Button/ R-Key - Open Replay Editor", Side.left, () => enabled);
            uiBoxKiwi.AddLabel("B-Button / Esc - Exit Replay Editor", Side.left, () => enabled);
            ModMenu.Instance.RegisterShowCursor(modId, () => {
                return (ReplayManager.CurrentState == ReplayState.PLAYBACK) ? 1 : 0;
            });
#endif
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            settings.Save(modEntry);
        }
        static void OnSettingsGUI(UnityModManager.ModEntry modEntry) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Maximum Time to be recorded: " + settings.MaxRecordedTime, GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            settings.MaxRecordedTime = GUILayout.HorizontalSlider(settings.MaxRecordedTime, 0, 300);
            GUILayout.EndHorizontal();
        }
    }
}
