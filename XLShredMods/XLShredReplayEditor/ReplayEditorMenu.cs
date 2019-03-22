using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XLShredReplayEditor {


    public class ReplayEditorMenu : MonoBehaviour {
        private enum MenuState {
            MainMenu, SaveMenu, Saving, LoadMenu, Loading, SettingsMenu
        }
        private MenuState _state;
        private MenuState State {
            get { return _state; }
            set {
                if (_state == value) return;
                _state = value;
                if (value == MenuState.LoadMenu)
                    FetchReplayFiles();
            }
        }
        private string fileName;
        private Rect DialogeBoxRect;
        Vector2 scrollPosition = Vector2.zero;

        private IEnumerable<string> replayDirectories;
        public void FetchReplayFiles() {
            try {
                replayDirectories = Directory.EnumerateDirectories(Main.settings.ReplaysDirectory);
            } catch (Exception e) {
                Main.modEntry.Logger.Error("Error fetching saved Replays from " + Main.settings.ReplaysDirectory + ": " + e.Message);
            }
        }

        public void Awake() {
            //this.camera = Camera.main;
            this.DialogeBoxRect = new Rect {
                center = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f),
                width = 330f,
                height = 200f
            };
            //this.renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            //this.fps = 30;
            //this.videoDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            //this.movieFileEnding = ".avi";
        }
        public void Toggle() {
            if (enabled)
                Close();
            else
                Open();
        }
        public void Open() {
            if (enabled) return;
            enabled = true;
            FetchReplayFiles();
            ReplayManager.Instance.cameraController.enabled = false;
            ReplayManager.Instance.enabled = false;
        }

        public void Close() {
            if (!enabled) return;
            State = MenuState.MainMenu;
            ReplayManager.Instance.cameraController.enabled = true;
            ReplayManager.Instance.enabled = true;
            enabled = false;
        }

        public void OnGUI() {
            switch (this.State) {
                case MenuState.MainMenu:
                    GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), DialogeBoxRect, MenuWindow, "ReplayEditor Menu");
                    break;
                case MenuState.LoadMenu:
                    GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), DialogeBoxRect, LoadWindow, "Load Replay");
                    break;
                case MenuState.SaveMenu:
                    GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), DialogeBoxRect, SaveWindow, "Save Replay");
                    break;
                case MenuState.SettingsMenu:
                    GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), DialogeBoxRect, SettingsWindow, "Settings");
                    break;
            }
            if (GUI.Button(new Rect(DialogeBoxRect.xMax - 20, DialogeBoxRect.y, 30, 30), "X")) {
                Close();
            }
        }

        public static string getFilePath(string filename) {
            string dir = Main.settings.ReplaysDirectory;
            if (dir.EndsWith("/") || dir.EndsWith("\\")) {
                return dir + filename;
            }
            return dir + "/" + filename;
        }
        #region GUI Windows
        public void MenuWindow(int id) {
            if (GUILayout.Button("Save")) {
                State = MenuState.SaveMenu;
            }
            if (GUILayout.Button("Load")) {
                State = MenuState.LoadMenu;
            }
            if (GUILayout.Button("Settings")) {
                State = MenuState.SettingsMenu;
            }
        }

        public void SettingsWindow(int id) {
            Main.OnSettingsGUI(Main.modEntry);
        }

        public void SaveWindow(int id) {
            GUILayout.Label("Directory: " + Main.settings.ReplaysDirectory);
            this.fileName = GUILayout.TextField(this.fileName);
            if (GUILayout.Button("Save")) {
                State = MenuState.Saving;
                var replayData = new ReplayData();
                replayData.SaveToFile(Main.settings.ReplaysDirectory + "\\" + this.fileName);
                State = MenuState.MainMenu;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back")) {
                State = MenuState.MainMenu;
                return;
            }
        }

        public void LoadWindow(int id) {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (string path in replayDirectories) {
                if (GUILayout.Button(path)) {
                    StartCoroutine(LoadReplayFromPath(path));
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back")) {
                State = MenuState.MainMenu;
                return;
            }
        }

        IEnumerator LoadReplayFromPath(string path) {
            State = MenuState.Loading;
            yield return ReplayData.LoadFromFile(path);
            Close();
        }
        #endregion
    }

}
