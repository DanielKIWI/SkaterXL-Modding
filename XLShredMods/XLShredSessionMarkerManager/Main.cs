using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using XLShredLib;
using XLShredLib.UI;

namespace XLShredSessionMarkerManager {
    using UI;

    static class Main {
        public static bool enabled;
        public static String modId;
        private static Traverse _respawnInstance = null;
        public static Traverse RespawnInstance {
            get {
                if (_respawnInstance == null) {
                    _respawnInstance = Traverse.Create(PlayerController.Instance.respawn);
                }
                return _respawnInstance;
            }
        }
        private static Traverse<Vector3[]> _setPosField = null;
        public static Traverse<Vector3[]> setPosField {
            get {
                if (_setPosField == null) {
                    _setPosField = RespawnInstance.Field<Vector3[]>("_setPos");
                }
                return _setPosField;
            }
        }
        private static Traverse<Quaternion[]> _setRotField = null;
        public static Traverse<Quaternion[]> setRotField {
            get {
                if (_setRotField == null) {
                    _setRotField = RespawnInstance.Field<Quaternion[]>("_setRot");
                }
                return _setRotField;
            }
        }
        public static string MarkerSavesDirectory = (Application.dataPath + "\\MarkerLocations").Replace('/', '\\');
        public static bool visible;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            modId = modEntry.Info.Id;
            modEntry.OnToggle = OnToggle;

            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddToggle("Session Marker Manager (T)", Side.left, () => enabled, false, (v) => visible = v);
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (value) {
                if (!Directory.Exists(MarkerSavesDirectory)) {
                    Directory.CreateDirectory(MarkerSavesDirectory);
                    Debug.Log("Created Directory at " + MarkerSavesDirectory);
                }
                LocationManagerUI.InstantiateInstance();
                ModMenu.Instance.RegisterShowCursor(Main.modId, () => LocationManagerUI.Instance.enabled ? 1 : 0);
            } else {
                ModMenu.Instance.UnregisterShowCursor(Main.modId);
                LocationManagerUI.DestroyInstance();
            }
            return true;
        }
    }
}
