using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using System;
using XLShredLib;
using XLShredLib.UI;

namespace XLShredReplayEditor {

    [Serializable]
    public class Settings : UnityModManager.ModSettings {

        public float MaxRecordedTime = 120f;
        public bool showRecGUI = false;
        public bool showLogo = true;

        public float TranslationSpeed = 5f;
        public float OrbitMoveSpeed = 5f;
        public float RotateSpeed = 20f;
        public float FOVChangeSpeed = 20f;
        public float DpadTickRate = 0.75f;
        public float PlaybackTimeJumpDelta = 5f;
        public float logoWidth = 75f;

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

            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddLabel("Start-Button/ R-Key - Open Replay Editor", Side.left, () => enabled);
            uiBoxKiwi.AddLabel("B-Button / Esc - Exit Replay Editor", Side.left, () => enabled);
            ModMenu.Instance.RegisterShowCursor(modId, () => {
                return (ReplayManager.CurrentState == ReplayState.PLAYBACK) ? 1 : 0;
            });
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            if (value == enabled) return true;
            enabled = value;
            if (enabled) {
                PromptController.Instance.menuthing.enabled = false; //Disabling the Tutorial
                ReplayManager rm = new GameObject("ReplayEditor").AddComponent<ReplayManager>();
            } else {
                PromptController.Instance.menuthing.enabled = true;
                ReplayManager.Instance?.Destroy();
            }

            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            settings.Save(modEntry);
            ReplayAudioRecorder.Instance?.CalcMaxTmpStreamLength();
        }
        static void OnSettingsGUI(UnityModManager.ModEntry modEntry) {
            GUILayout.Label("Website: https://github.com/DanielKIWI/SkaterXL-Modding");
            GUILayout.Label("Would love to see the logo in your videos. But its your choice ;)");
            GUILayout.BeginHorizontal();
            settings.showLogo = GUILayout.Toggle(settings.showLogo, "Show Logo");
            GUILayout.FlexibleSpace();
            GUILayout.Label(ReplaySkin.DefaultSkin.kiwiCamTexture, GUILayout.Width(settings.logoWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            SettingSliderGUI("Logo Size", () => settings.logoWidth, (v) => settings.logoWidth = v, 25, 100);
            settings.showRecGUI = GUILayout.Toggle(settings.showRecGUI, "Show 'REC'-Icon");
            
            GUILayout.Space(8);
            SettingSliderGUI("Free Move Speed", () => settings.TranslationSpeed, (v) => settings.TranslationSpeed = v, 0, 100);
            SettingSliderGUI("Free Rotate Speed", () => settings.RotateSpeed, (v) => settings.RotateSpeed = v, 0, 100);
            SettingSliderGUI("Orbit Move Speed", () => settings.OrbitMoveSpeed, (v) => settings.OrbitMoveSpeed = v, 0, 100);
            SettingSliderGUI("FOV Change Speed", () => settings.FOVChangeSpeed, (v) => settings.FOVChangeSpeed = v, 0, 100);
            GUILayout.Space(8);
            SettingSliderGUI("Max Record Time", () => settings.MaxRecordedTime, (v) => settings.MaxRecordedTime = v, 0, 300);
        }
        static void SettingSliderGUI(string name, Func<float> getter, Action<float> setter, float min, float max) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(200));
            GUILayout.Space(8);
            float value;
            if (float.TryParse(GUILayout.TextField(getter().ToString("0.00"), GUILayout.Width(50)), out value)) {
                setter(value);
            }
            setter(GUILayout.HorizontalSlider(getter(), min, max));
            GUILayout.EndHorizontal();
        }
    }
}
