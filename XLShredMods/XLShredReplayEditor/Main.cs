using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using System;
using XLShredLib;
using XLShredLib.UI;
using System.IO;

namespace XLShredReplayEditor {

    [Serializable]
    public class Settings : UnityModManager.ModSettings {

        public float MaxRecordedTime = 120f;
        public bool showLogo = true;
        public bool showControllsHelp = true;

        public float TranslationSpeed = 5f;
        public float OrbitMoveSpeed = 5f;
        public float RotateSpeed = 20f;
        public float FOVChangeSpeed = 20f;
        public float DpadTickRate = 0.75f;
        public float PlaybackTimeJumpDelta = 5f;
        public float logoWidth = 75f;
        public float CameraSensorSize = 8.47f;
        public string ReplaysDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\SkaterXL\\Replays";

        public override void Save(UnityModManager.ModEntry modEntry) {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    static class Main {

        public static bool enabled;
        public static Settings settings;
        public static String modId;
        public static UnityModManager.ModEntry modEntry;

        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            Main.modEntry = modEntry;
            settings = Settings.Load<Settings>(modEntry);
            if (!Directory.Exists(settings.ReplaysDirectory)) {
                try {
                    Directory.CreateDirectory(settings.ReplaysDirectory);
                } catch (Exception e) {
                    Main.modEntry.Logger.Error("Error creating Directory at " + Main.settings.ReplaysDirectory + ": " + e.Message);
                    ReplayDirectoryExists = false;
                }
            }
            modId = modEntry.Info.Id;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnSettingsGUI;
            _replaysDirectory = settings.ReplaysDirectory;

            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddLabel("Start-Button/ R-Key - Open Replay Editor", Side.left, () => enabled);
            uiBoxKiwi.AddLabel("B-Button / Esc - Exit Replay Editor", Side.left, () => enabled);
            ModMenu.Instance.RegisterShowCursor(modId, () => {
                return (ReplayManager.CurrentState == ReplayState.PLAYBACK) ? 1 : 0;
            });
            XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", false);
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
        public static void OnSettingsGUI(UnityModManager.ModEntry modEntry) {
            if (GUILayout.Button("Website: https://github.com/DanielKIWI/SkaterXL-Modding")) {
                Application.OpenURL("https://github.com/DanielKIWI/SkaterXL-Modding");
            }
            GUILayout.Label("Would love to see the logo in your videos. But its your choice ;)");
            GUILayout.BeginHorizontal();
            settings.showLogo = GUILayout.Toggle(settings.showLogo, "Show Logo");
            GUILayout.FlexibleSpace();
            GUILayout.Label(ReplaySkin.DefaultSkin.kiwiCamTexture, GUILayout.Width(settings.logoWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            FloatSettingSliderGUI("Logo Size", () => settings.logoWidth, (v) => settings.logoWidth = v, 25, 100);

            GUILayout.Space(8);
            FloatSettingSliderGUI("Free Move Speed", () => settings.TranslationSpeed, (v) => settings.TranslationSpeed = v, 0, 100);
            FloatSettingSliderGUI("Free Rotate Speed", () => settings.RotateSpeed, (v) => settings.RotateSpeed = v, 0, 100);
            FloatSettingSliderGUI("Orbit Move Speed", () => settings.OrbitMoveSpeed, (v) => settings.OrbitMoveSpeed = v, 0, 100);
            FloatSettingSliderGUI("FOV Change Speed", () => settings.FOVChangeSpeed, (v) => settings.FOVChangeSpeed = v, 0, 100);
            GUILayout.Space(8);
            FloatSettingSliderGUI("Camera sensor size in mm \n(used for focalLength calculation)", () => settings.CameraSensorSize, (v) => settings.CameraSensorSize = v, 0, 100);
            GUILayout.Space(8);
            FloatSettingSliderGUI("Max Record Time", () => settings.MaxRecordedTime, (v) => settings.MaxRecordedTime = v, 0, 300);
            GUILayout.Space(8);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Replays Directory Path");
            GUILayout.Space(8);
            ReplaysDirectory = GUILayout.TextField(ReplaysDirectory, GUILayout.ExpandWidth(true));
            GUIContent content = new GUIContent("Save and Create Directory");
            if (!ReplayDirectoryExists && GUILayout.Button(content, GUILayout.Width(GUI.skin.button.CalcSize(content).x))) {
                settings.ReplaysDirectory = _replaysDirectory;
                try {
                    Directory.CreateDirectory(ReplaysDirectory);
                } catch (Exception e) {
                    modEntry.Logger.Log("Can't create Directory at '" + ReplaysDirectory + "'! Error: " + e.Message);
                }
            }
            GUILayout.EndHorizontal();
        }
        private static string _replaysDirectory;
        private static string ReplaysDirectory {
            get { return _replaysDirectory; }
            set {
                if (_replaysDirectory == value) return;
                _replaysDirectory = value;
                ReplayDirectoryExists = Directory.Exists(value);
                if (ReplayDirectoryExists)
                    Main.settings.ReplaysDirectory = value;
            }
        }
        private static bool ReplayDirectoryExists;

        static void FloatSettingSliderGUI(string name, Func<float> getter, Action<float> setter, float min, float max) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (float.TryParse(GUILayout.TextField(getter().ToString("0.00"), GUILayout.Width(50)), out float value)) {
                setter(value);
            }
            setter(GUILayout.HorizontalSlider(getter(), min, max, GUILayout.MinWidth(600f)));
            GUILayout.EndHorizontal();
        }
    }
}
