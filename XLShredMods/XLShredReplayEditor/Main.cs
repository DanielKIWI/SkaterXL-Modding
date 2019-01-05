using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using System;
#if !STANDALONE
using XLShredLib;
#endif

namespace XLShredReplayEditor {

    [Serializable]
    public class Settings : UnityModManager.ModSettings {

        public bool adjustAudioPitch = true;
        public float MaxRecordedTime = 300f;
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
                Debug.LogException(e);
            }
            ReplayManager rm = new GameObject("ReplayEditor").AddComponent<ReplayManager>();
            PromptController.Instance.menuthing.enabled = false;
#if !STANDALONE
            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddLabel("Start - Replay Editor", ModUIBox.Side.right, () => UnityModManager.FindMod("XLShredReplayEditor").Enabled);
            ModMenu.Instance.RegisterShowCursor("XLShredReplayEditor", () => {
                return ReplayManager.CurrentState == ReplayState.PLAYBACK ? 1 : 0;
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
    }
}
