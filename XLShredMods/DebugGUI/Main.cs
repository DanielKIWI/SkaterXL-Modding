﻿using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using System;
using GUILayoutLib;

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
        public static bool guiVisible = true;
        public static GameObject DebugGuiObject;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            settings = Settings.Load<Settings>(modEntry);

            modId = modEntry.Info.Id;

            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnGUI += OnGUI;
            modEntry.OnToggle = OnToggle;
        }
        private static GUIValueCache _cache = new GUIValueCache();
        private static void OnGUI(UnityModManager.ModEntry obj) {
            GUILayoutHelper.FloatField("MessageLifeTime", _cache, () => settings.MessageLifeTime, v => settings.MessageLifeTime = v, 0, 600);
            GUILayoutHelper.IntField("MaxLogsCount", _cache, () => settings.MaxLogsCount, v => settings.MaxLogsCount = v, 0, 600);
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                unityLogHandler = Debug.unityLogger.logHandler;
                DebugGuiObject = new GameObject("DebugGUI");
                DebugGuiObject.AddComponent<DebugConsoleGUI>();
                DebugGuiObject.AddComponent<DebugHierarchyGUI>();
                DebugConsoleGUI.Instance.parentHandler = unityLogHandler;
                Debug.unityLogger.logHandler = DebugConsoleGUI.Instance;

            } else {
                if (DebugConsoleGUI.Instance != null) {
                    Debug.unityLogger.logHandler = unityLogHandler;
                    DebugConsoleGUI.Instance.parentHandler = null;
                    GameObject.Destroy(DebugGuiObject);
                }
            }
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            settings.Save(modEntry);
        }
    }
}
