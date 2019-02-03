using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using System;
using System.Collections.Generic;

namespace XLShredRealisticTrucks {
    using Extensions;
    using UI;

    [Serializable]
    public class Settings : UnityModManager.ModSettings {

        public float PowerSlideFriction = 0.1f;
        public float RollSideWaysFriction = 0.7f;
        public float RollFriction = 0f;

        public float FrontTruckDamper = BoardControllerExtension.FrontTruckDamper;
        public float FrontTruckSpring = BoardControllerExtension.FrontTruckSpring;
        public Vector3 FrontTruckKingPinEuler = BoardControllerExtension.FrontTruckKingPinEuler;

        public float BackTruckDamper = BoardControllerExtension.BackTruckDamper;
        public float BackTruckSpring = BoardControllerExtension.BackTruckSpring;
        public Vector3 BackTruckKingPinEuler = BoardControllerExtension.BackTruckKingPinEuler;

        public float MaxWeightOnBoardXOffset = 0.06f;

        //public float currentBackWheelsFriction;
        //public float currentFrontWheelsFriction;
        //public bool useTraditionalRotating;
        public bool editBothTrucksTogether;
        public float ManualWeightOnBoardYOffset = 0.225f;
        public Vector2 GUIWindowPosition = new Vector2(Screen.width - 210, 10);
        public Vector2 GUIWindowSize = new Vector2(300, 0);

        public override void Save(UnityModManager.ModEntry modEntry) {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    static class Main {
        internal static float currentFrontWheelsFriction;
        internal static float currentBackWheelsFriction;
        public static bool enabled;
        public static Settings settings;
        private static HarmonyInstance harmony;
        private static bool isPatched = false;
        private static BoardSettingsGUI boardSettingsGUI;
        public static UnityModManager.ModEntry modEntry;

        static bool Load(UnityModManager.ModEntry modEntry) {
            Main.modEntry = modEntry;
            settings = Settings.Load<Settings>(modEntry);
            harmony = HarmonyInstance.Create(modEntry.Info.Id);
            modEntry.OnToggle = OnToggle;
            return true;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                boardSettingsGUI = new GameObject("BoardSettingsGUI").AddComponent<BoardSettingsGUI>();
                GameObject.DontDestroyOnLoad(boardSettingsGUI.gameObject);
                if (!isPatched)
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                isPatched = true;
            } else {
                if (boardSettingsGUI != null)
                    GameObject.Destroy(boardSettingsGUI.gameObject);
                if (isPatched)
                    harmony.UnpatchAll(harmony.Id);
                isPatched = false;
            }
            return true;
        }
    }
}
