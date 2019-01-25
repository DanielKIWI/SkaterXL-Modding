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

namespace XLShredSessionMarkerManager {
    using UI;

    [Serializable]
    public class Settings : UnityModManager.ModSettings {
        public bool TeleportDirectly = true;
        public Rect WindowRect = new Rect { 
            center = new Vector2(Screen.width / 2f, Screen.height / 2f),
            width = 400,
            height = 400
        };
        public override void Save(UnityModManager.ModEntry modEntry) {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public void MoveIntoScreen() {
            bool changed = false;
            if (WindowRect.xMin < 0) {
                WindowRect.xMin = 0;
                changed = true;
            }
            if (WindowRect.xMax > Screen.width) {
                WindowRect.xMax = Screen.width;
                changed = true;
            }
            if (WindowRect.yMin < 0) {
                WindowRect.yMin = 0;
                changed = true;
            }
            if (WindowRect.yMax > Screen.height) {
                WindowRect.yMax = Screen.height;
                changed = true;
            }
            if (changed) {
                Save(Main.modEntry);
            }
        }
    }
    static class Main {
        public static bool enabled;
        public static Settings settings;
        public static String modId;
        public static UnityModManager.ModEntry modEntry;
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
        public static string MarkerSavesRootDirectory = (Application.dataPath + "\\MarkerLocations").Replace('/', '\\');
        public static string MarkerSavesDirectory = (Application.dataPath + "\\MarkerLocations\\Courthouse").Replace('/', '\\');

        private static Scene startScene;
        public static bool visible;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            settings = Settings.Load<Settings>(modEntry);
            Main.modEntry = modEntry;
            modId = modEntry.Info.Id;
            modEntry.OnToggle = OnToggle;

            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddToggle("Session Marker Manager (T)", Side.left, () => enabled, false, (v) => visible = v);

            startScene = SceneManager.GetSceneAt(0);

            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modId);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (value) {
                if (!Directory.Exists(MarkerSavesRootDirectory)) {
                    Directory.CreateDirectory(MarkerSavesRootDirectory);
                    Debug.Log("Created Directory at " + MarkerSavesRootDirectory);
                }
                SceneManager.sceneLoaded += OnSceneLoaded;
                LocationManagerUI.InstantiateInstance();
                ModMenu.Instance.RegisterShowCursor(Main.modId, () => LocationManagerUI.Instance.enabled ? 1 : 0);
                OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            } else {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                ModMenu.Instance.UnregisterShowCursor(Main.modId);
                LocationManagerUI.DestroyInstance();
            }
            return true;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            Debug.Log("Scene " + scene.name + " (" + scene.path + ") loaded with mode: " + mode.ToString());
            if (scene == startScene) {
                Debug.Log("Loaded Scene is start scene");
                MarkerSavesDirectory = (Application.dataPath + "\\MarkerLocations\\Courthouse").Replace('/', '\\');
                if (!Directory.Exists(MarkerSavesDirectory)) {
                    Directory.CreateDirectory(MarkerSavesDirectory);
                    Debug.Log("Created Directory at " + MarkerSavesDirectory);
                }
                LocationManagerUI.Instance.UpdateMarkerList();
            }
        }
        public static void LoadedAssetBundle(AssetBundle bundle, string path) {
            string assetBundleName = path.Substring(path.LastIndexOf('\\') + 1);
            if (assetBundleName.Contains(".")) {
                assetBundleName.Remove(assetBundleName.IndexOf('.'));
            }
            Main.MarkerSavesDirectory = Main.MarkerSavesRootDirectory + '\\' + assetBundleName + '\\';
            Debug.Log("LoadedAssetBundle " + bundle.name + " - " + assetBundleName + " (" + path + ") -> MarkerSavesDirectory: " + MarkerSavesDirectory);
            if (!Directory.Exists(MarkerSavesDirectory)) {
                Directory.CreateDirectory(MarkerSavesDirectory);
                Debug.Log("Created Directory at " + MarkerSavesDirectory);
            }
            LocationManagerUI.Instance.UpdateMarkerList();
        }
    }
}
