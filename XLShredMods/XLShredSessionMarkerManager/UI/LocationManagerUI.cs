using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XLShredLib;

namespace XLShredSessionMarkerManager.UI {
    class LocationManagerUI: MonoBehaviour {
        #region static Instance stuff
        private static LocationManagerUI _instance;
        public static LocationManagerUI Instance {
            get { return _instance; }
        }
        public static void InstantiateInstance() {
            if (_instance != null) return;
            _instance = new GameObject("LocationManagerUI").AddComponent<LocationManagerUI>();
            DontDestroyOnLoad(_instance.gameObject);
            _instance.enabled = true;
        }
        public static void DestroyInstance() {
            if (_instance == null) return;
            Destroy(_instance.gameObject);
        }
        #endregion
        
        private string toDeleteName = null;
        private string toDeletePath = null;
        private string saveName;
        public bool TeleportDirectly {
            get { return Main.settings.TeleportDirectly; }
            set {
                if (Main.settings.TeleportDirectly != value) {
                    Main.settings.TeleportDirectly = value;
                    Main.settings.Save(Main.modEntry);
                }
            }
        }
        private Vector3 scrollPosition;

        private Rect WindowRect {
            get {return Main.settings.WindowRect; }
            set {
                if (Main.settings.WindowRect != value) {
                    Main.settings.WindowRect = value;
                    Main.settings.Save(Main.modEntry);
                }
            }
        }
        public string[] MarkerPaths;

        public void Awake() {
            MigrateOldLocations();
            UpdateMarkerList();
        }
        public void MigrateOldLocations() {
            if (!Directory.Exists(Main.MarkerSavesRootDirectory)) return;
            var oldFiles = Directory.GetFiles(Main.MarkerSavesRootDirectory, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string path in oldFiles) {
                string newPath = Main.MarkerSavesDirectory + path.Substring(path.LastIndexOf('\\') + 1);
                File.Move(path, newPath);
            }
        }
        public void UpdateMarkerList() {
            MarkerPaths = Directory.GetFiles(Main.MarkerSavesDirectory, "*.json", SearchOption.TopDirectoryOnly);
        }
        public void Update() {
            if (Input.GetKeyDown(KeyCode.T)) {
                Main.visible = !Main.visible;
            }
        }
        public void OnGUI() {
            if (!Main.visible) return;
            if (toDeleteName != null) {
                GUI.Window(GUIUtility.GetControlID(FocusType.Passive), new Rect(Screen.width / 2f - 250, Screen.height / 2f - 40, 500, 80), DoDeleteWindow, "Are you sure to delete Marker: " + toDeleteName);
            } else {
                WindowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), WindowRect, DoMainWindow, "Saved Session Markers");
            }
        }
        void DoDeleteWindow(int windowID) {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Yes")) {
                    File.Delete(toDeletePath);
                    UpdateMarkerList();
                    toDeleteName = null;
                    toDeletePath = null;
                }
                if (GUILayout.Button("Cancel")) {
                    toDeleteName = null;
                    toDeletePath = null;
                }
            }
            GUILayout.EndHorizontal();
        }
        // Make the contents of the window
        void DoMainWindow(int windowID) {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    saveName = GUILayout.TextField(saveName);
                    if (GUILayout.Button("Save", GUILayout.Width(80f))) {
                        var t = new TeleporterLocation(saveName);
                        t.SaveToDirectory(Main.MarkerSavesDirectory);
                        UpdateMarkerList();
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                TeleportDirectly = GUILayout.Toggle(TeleportDirectly, "Directly teleport to marker");
                if (GUILayout.Button("↺", GUILayout.Width(20f))) {
                    UpdateMarkerList();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10f);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                {
                    foreach (string path in MarkerPaths) {
                        DrawGUIForMarker(path);
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }
        void DrawGUIForMarker(string path) {
            string name = path.Substring(path.LastIndexOf('\\') + 1);
            name = name.Remove(name.LastIndexOf(".json"));
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(name)) {
                    TeleporterLocation.LoadFromFile(path).Apply();
                    if (TeleportDirectly) {
                        PlayerController.Instance.respawn.DoRespawn();
                    }
                }
                if (GUILayout.Button("X", GUILayout.Width(20f))) {
                    toDeleteName = name;
                    toDeletePath = path;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
