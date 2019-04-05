using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace XLShredReplayEditor {
    public class ReplayEditorMenu {
        private string fileName;
        private Rect MenuRect;
        private Rect SaveRect;
        private Rect LoadRect;
        private Rect SettingsRect;
        private Rect CurrentRect {
            get {
                switch (ReplayManager.CurrentState) {
                    case ReplayState.MainMenu:
                        return MenuRect;
                    case ReplayState.LoadMenu:
                        return LoadRect;
                    case ReplayState.SaveMenu:
                        return SaveRect;
                    case ReplayState.SettingsMenu:
                        return SettingsRect;
                    default: return new Rect();
                }
            }
        }

        Vector2 scrollPosition = Vector2.zero;

        private IEnumerable<string> replayNames;

        public ReplayEditorMenu() {
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

        public void OnStateChanged(ReplayState newState, ReplayState oldState) {
            if (newState == ReplayState.LoadMenu)
                FetchReplayFiles();
            if (newState == ReplayState.SaveMenu)
                fileName = "Replay_" +
                    DateTime.Now.Year.ToString("0000") + "-" +
                    DateTime.Now.Month.ToString("00") + "-" +
                    DateTime.Now.Day.ToString("00") + "_" +
                    DateTime.Now.Hour.ToString("00") + "-" +
                    DateTime.Now.Minute.ToString("00") + "-" +
                    DateTime.Now.Second.ToString("00");
        }

        public void FetchReplayFiles() {
            try {
                replayNames = Directory.EnumerateDirectories(Main.settings.ReplaysDirectory).Select(delegate (string path) {
                    int i = path.LastIndexOf('\\');
                    return path.Substring(i + 1);
                });
                LoadRect.height = Mathf.Max(500f, replayNames.Count() * 25f + 80f);
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
        public static bool isValidFileName(string fileName) {
            return !(
                fileName.Contains("/") ||
                fileName.Contains("\\") ||
                fileName.Contains(":") ||
                fileName.Contains("*") ||
                fileName.Contains("?") ||
                fileName.Contains("\"") ||
                fileName.Contains("<") ||
                fileName.Contains(">") ||
                fileName.Contains("|")
                );
        }

        public void CheckInput(ReplayState state) {
            if (Input.GetKeyDown(KeyCode.Escape) || PlayerController.Instance.inputController.player.GetButtonDown("B")) {
                if (state == ReplayState.MainMenu) {
                    ReplayManager.Instance.CloseMenu();
                } else if (state.IsMenuOpen() && state.CanBeChange()) {
                    ReplayManager.CurrentState = ReplayState.MainMenu;
                }
            }
            if (state == ReplayState.SaveMenu) {
                if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) {
                    Save();
                }
            }
        }

        public void DrawGUI(ReplayState state) {
            switch (state) {
                case ReplayState.MainMenu:
                    MenuRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), MenuRect, MenuWindow, "ReplayEditor Menu");
                    break;
                case ReplayState.LoadMenu:
                    LoadRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), LoadRect, LoadWindow, "Load Replay");
                    break;
                case ReplayState.SaveMenu:
                    SaveRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), SaveRect, SaveWindow, "Save Replay");
                    break;
                case ReplayState.SettingsMenu:
                    SettingsRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), SettingsRect, SettingsWindow, "Settings");
                    break;
            }
            if (GUI.Button(new Rect(CurrentRect.xMax, CurrentRect.y, 30, 30), "X")) {
                ReplayManager.CurrentState = ReplayState.Playback;
            }
        }

        #region GUI Windows
        public void MenuWindow(int id) {
            if (GUILayout.Button("Save", GUILayout.Height(25))) {
                ReplayManager.CurrentState = ReplayState.SaveMenu;
            }
            if (GUILayout.Button("Load", GUILayout.Height(25))) {
                ReplayManager.CurrentState = ReplayState.LoadMenu;
            }
            if (GUILayout.Button("Settings", GUILayout.Height(25))) {
                ReplayManager.CurrentState = ReplayState.SettingsMenu;
            }
        }

        public void SettingsWindow(int id) {
            Main.OnSettingsGUI(Main.modEntry);
        }

        public void SaveWindow(int id) {
            GUILayout.Label("Directory: " + Main.settings.ReplaysDirectory);
            this.fileName = GUILayout.TextField(this.fileName, GUILayout.Height(25));
            if (!isValidFileName(fileName)) {
                GUILayout.Label("Invalid Filename!!!");
            } else {
                if (GUILayout.Button("Save", GUILayout.Height(25))) {
                    Save();
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back")) {
                ReplayManager.CurrentState = ReplayState.MainMenu;
            }
        }

        private void Save() {
            ReplayManager.CurrentState = ReplayState.SavingReplay;
            var replayData = new ReplayData();
            replayData.SaveToFile(Main.settings.ReplaysDirectory + "\\" + this.fileName);
            ReplayManager.CurrentState = ReplayState.MainMenu;
        }

        public void LoadWindow(int id) {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (string replayName in replayNames) {
                if (GUILayout.Button(replayName, GUILayout.Height(25))) {
                    string path = Main.settings.ReplaysDirectory + "\\" + replayName;
                    ReplayManager.Instance.StartCoroutine(LoadReplayFromPath(path));
                }
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back")) {
                ReplayManager.CurrentState = ReplayState.MainMenu;
            }
        }

        IEnumerator LoadReplayFromPath(string path) {
            ReplayManager.CurrentState = ReplayState.LoadingReplay;
            yield return ReplayData.LoadFromFile(path);
            ReplayManager.CurrentState = ReplayState.Playback;
        }
        #endregion
    }
}
