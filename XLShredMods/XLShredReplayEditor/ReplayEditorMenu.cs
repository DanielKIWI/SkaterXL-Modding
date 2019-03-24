using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace XLShredReplayEditor {


    public class ReplayEditorMenu : MonoBehaviour {
        private enum MenuState {
            MainMenu = -1,
            SaveMenu = 0, LoadMenu = 1, SettingsMenu = 2,
            Saving = 10, Loading = 11
        }
        private MenuState _state = MenuState.MainMenu;
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
        private Rect MenuRect;
        private Rect SaveRect;
        private Rect LoadRect;
        private Rect SettingsRect;
        private Rect CurrentRect {
            get {
                switch (this.State) {
                    case MenuState.MainMenu:
                        return MenuRect;
                    case MenuState.LoadMenu:
                        return LoadRect;
                    case MenuState.SaveMenu:
                        return SaveRect;
                    case MenuState.SettingsMenu:
                        return SettingsRect;
                    default: return new Rect();
                }
            }
        }

        Vector2 scrollPosition = Vector2.zero;

        private IEnumerable<string> replayNames;
        public void FetchReplayFiles() {
            try {
                replayNames = Directory.EnumerateDirectories(Main.settings.ReplaysDirectory).Select(delegate(string path) {
                    int i = path.LastIndexOf('\\');
                    return path.Substring(i + 1);
                });
                LoadRect.height = Mathf.Max(500f, replayNames.Count() * 20f + 80f);
            } catch (Exception e) {
                Main.modEntry.Logger.Error("Error fetching saved Replays from " + Main.settings.ReplaysDirectory + ": " + e.Message);
            }
        }

        public static string getFilePath(string filename) {
            string dir = Main.settings.ReplaysDirectory;
            if (dir.EndsWith("/") || dir.EndsWith("\\")) {
                return dir + filename;
            }
            return dir + "/" + filename;
        }

        public void Awake() {
            Vector2 center = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
            MenuRect = new Rect(0, 0, 330f, 200f) {
                center = center
            };
            SaveRect = new Rect(0, 0, 330f, 200f) {
                center = center
            };
            LoadRect = new Rect(0, 0, 330f, 400f) {
                center = center
            };
            SettingsRect = new Rect(0, 0, 1000f, 400f) {
                center = center
            };
        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) || PlayerController.Instance.inputController.player.GetButtonDown("B")) {
                if (State == MenuState.MainMenu) {
                    Close();
                }
                if ((int)State < 10) {  //Not loading or saving
                    State = MenuState.MainMenu;
                }
            }
        }

        public void Toggle() {
            if (enabled)
                Close();
            else
                Open();
        }

        public void Save() {
            if (!enabled) {
                State = MenuState.SaveMenu;
                Open();
            } else if ((int)State < 10) {
                State = MenuState.SaveMenu;
            }
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
                    MenuRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), MenuRect, MenuWindow, "ReplayEditor Menu");
                    break;
                case MenuState.LoadMenu:
                    LoadRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), LoadRect, LoadWindow, "Load Replay");
                    break;
                case MenuState.SaveMenu:
                    SaveRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), SaveRect, SaveWindow, "Save Replay");
                    break;
                case MenuState.SettingsMenu:
                    SettingsRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), SettingsRect, SettingsWindow, "Settings");
                    break;
            }
            if (GUI.Button(new Rect(CurrentRect.xMax, CurrentRect.y, 30, 30), "X")) {
                Close();
            }
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
            if (fileName.Contains("/") || 
                fileName.Contains("\\") || 
                fileName.Contains(":") || 
                fileName.Contains("*") || 
                fileName.Contains("?") || 
                fileName.Contains("\"") || 
                fileName.Contains("<") || 
                fileName.Contains(">") || 
                fileName.Contains("|")) {
                GUILayout.Label("Invalid Filename!!!");
            } else {
                if (GUILayout.Button("Save")) {
                    State = MenuState.Saving;
                    var replayData = new ReplayData();
                    replayData.SaveToFile(Main.settings.ReplaysDirectory + "\\" + this.fileName);
                    State = MenuState.MainMenu;
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back")) {
                State = MenuState.MainMenu;
                return;
            }
        }

        public void LoadWindow(int id) {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (string replayName in replayNames) {
                if (GUILayout.Button(replayName, GUILayout.Height(25))) {
                    string path = Main.settings.ReplaysDirectory + "\\" + replayName;
                    StartCoroutine(LoadReplayFromPath(path));
                }
            }
            GUILayout.EndScrollView();
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
