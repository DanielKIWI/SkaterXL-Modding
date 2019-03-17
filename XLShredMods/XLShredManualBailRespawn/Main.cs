using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Harmony12;
using UnityModManagerNet;
using XLShredLib;
using XLShredLib.UI;
using System.Reflection;

namespace XLShredManualBailRespawn {

    static class Main {
        public static bool enabled;
        public static String modId;
        public static bool visible;
        private static HarmonyInstance harmonyInstance;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            modId = modEntry.Info.Id;
            modEntry.OnToggle = OnToggle;

            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddToggle("Manual Bail Respawn (Xbox: A, PS4: X)", Side.left, () => enabled, false, (v) => visible = v);

            harmonyInstance = HarmonyInstance.Create(modId);
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            } else {
                if (harmonyInstance != null)
                    harmonyInstance.UnpatchAll(harmonyInstance.Id);
            }
            return true;
        }
    }
}
