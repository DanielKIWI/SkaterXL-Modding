﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
            _instance.enabled = true;
        }
        public static void DestroyInstance() {
            if (_instance == null) return;
            Destroy(_instance.gameObject);
        }
        #endregion
        
        private string toDeleteName = null;
        private string saveName;
        private bool teleportDirectly;
        private Vector3 scrollPosition;
        private Rect windowRect = new Rect() {
            center = new Vector2(Screen.width / 2f, Screen.height / 2f),
            width = 400,
            height = 400
        };
        public string[] MarkerPaths;

        public void Awake() {
            UpdateMarkerList();
        }
        public void UpdateMarkerList() {
            MarkerPaths = Directory.GetFiles(Main.MarkerSavesDirectory, "*.json", SearchOption.AllDirectories);
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
                windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), windowRect, DoMainWindow, "Saved Session Markers");
            }
        }
        void DoDeleteWindow(int windowID) {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Yes")) {
                    File.Delete(toDeleteName);
                    UpdateMarkerList();
                    toDeleteName = null;
                }
                if (GUILayout.Button("Cancel")) {
                    toDeleteName = null;
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
                teleportDirectly = GUILayout.Toggle(teleportDirectly, "Directly teleport to marker");
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
                    if (teleportDirectly) {
                        PlayerController.Instance.respawn.DoRespawn();
                    }
                }
                if (GUILayout.Button("X", GUILayout.Width(20f))) {
                    toDeleteName = name;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
