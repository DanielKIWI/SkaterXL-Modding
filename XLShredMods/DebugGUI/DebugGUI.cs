using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DebugGUI {
    public class DebugGUI : MonoBehaviour, ILogHandler {

        public ILogHandler parentHandler;
        private static DebugGUI _instance;
        private GUIStyle font;
        private List<DebugGUI.Message> messages;
        private bool consoleVisible = true;
        private GUIStyle boxStyle;
        private bool b_guiStyled;

        private Dictionary<LogType, bool> showLogType;
        public void Awake() {
            this.messages = new List<DebugGUI.Message>();
            LogType[] logTypes = Enum.GetValues(typeof(LogType)).Cast<LogType>().ToArray();
            showLogType = new Dictionary<LogType, bool>();
            foreach (var type in logTypes) {
                showLogType.Add(type, true);
            }
            UnityEngine.Debug.unityLogger.filterLogType = LogType.Log;
            UnityEngine.Debug.unityLogger.logEnabled = true;
        }


        public void OnGUI() {
            if (!this.consoleVisible) {
                return;
            }
            if (!this.b_guiStyled) {
                this.b_guiStyled = true;
                this.font = new GUIStyle(GUI.skin.label);
                this.font.fontSize = 20;
                this.font.wordWrap = true;
                this.boxStyle = new GUIStyle(GUI.skin.box);
            }
            float boxMinY = (float)Screen.height - 10f;
            float width = (float)Screen.width - 20f;
            if (this.messages.Count > 0) {
                float num = 30f;
                float height = num;
                foreach (DebugGUI.Message message in this.messages) {
                    GUIContent content = new GUIContent(message.text);
                    height += this.font.CalcHeight(content, width - 20f);
                }
                if (this.font == null) {
                    throw new Exception("font is null");
                }
                if (this.boxStyle == null) {
                    throw new Exception("boxStyle is null");
                }
                boxMinY = (float)Screen.height - height - 10f;
                GUI.Box(new Rect(10f, boxMinY, width, height), "Console", this.boxStyle);
                foreach (DebugGUI.Message message2 in this.messages) {
                    this.font.normal.textColor = message2.color;
                    float num5 = this.font.CalcHeight(new GUIContent(message2.text), width - 20f);
                    GUI.Label(new Rect(20f, boxMinY + num, width - 20f, num5), message2.text, this.font);
                    num += num5;
                }
            }
            LogType[] logTypes = Enum.GetValues(typeof(LogType)).Cast<LogType>().ToArray();
            float w = 100f;
            float x = 20f;
            foreach (var type in logTypes) {
                showLogType[type] = GUI.Toggle(new Rect(x, boxMinY - 20f, w, 20f), showLogType[type], Enum.GetName(typeof(LogType), type));
                x += w;
            }
        }
        
        public static DebugGUI Instance {
            get {
                if (DebugGUI._instance == null) {
                    DebugGUI._instance = new GameObject("GUIConsole").AddComponent<DebugGUI>();
                }
                return DebugGUI._instance;
            }
        }
        private void Update() {
            if (this.messages.Count > 0 && Time.unscaledTime - this.messages[0].time > Main.settings.MessageLifeTime) {
                this.messages.RemoveAt(0);
            }
            if (Input.GetKeyDown(KeyCode.F9)) {
                this.consoleVisible = !this.consoleVisible;
            }
            if (Input.GetKeyDown(KeyCode.Delete)) {
                if (this.messages == null) {
                    this.messages = new List<DebugGUI.Message>();
                } else {
                    this.messages.Clear();
                }
            }
        }
        
        private void AddMessage(string message, Color color) {
            this.messages.Add(new DebugGUI.Message(message, color));
            if (this.messages.Count > Main.settings.MaxLogsCount) {
                this.messages.RemoveAt(0);
            }
        }
        private Color colorForLogType(LogType type) {
            switch (type) {
                case LogType.Exception: return Color.red;
                case LogType.Error: return Color.red;
                case LogType.Assert: return Color.yellow;
                case LogType.Warning: return Color.yellow;
                case LogType.Log: return Color.white;
                default: return Color.white;
            }
        }
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
            parentHandler.LogFormat(logType, context, format, args);
            if (!Main.enabled) return;
            if (showLogType[logType]) {
                AddMessage(Enum.GetName(typeof(LogType), logType) + ": " + String.Format(format, args), colorForLogType(logType));
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context) {
            parentHandler.LogException(exception, context);
            if (!Main.enabled) return;
            StackTrace stackTrace = new StackTrace(exception, true);
            string text = "";
            for (int i = 0; i < Mathf.Min(stackTrace.FrameCount, 4); i++) {
                text = text + "\n" + stackTrace.GetFrame(i).ToString();
            }
        }


        // Token: 0x02000234 RID: 564
        private struct Message {
            // Token: 0x06001725 RID: 5925 RVA: 0x00011973 File Offset: 0x0000FB73
            public Message(string txt) {
                this.text = txt;
                this.time = Time.unscaledTime;
                this.color = Color.white;
            }

            // Token: 0x06001726 RID: 5926 RVA: 0x00011992 File Offset: 0x0000FB92
            public Message(string txt, Color color) {
                this.text = txt;
                this.time = Time.unscaledTime;
                this.color = color;
            }

            // Token: 0x04001141 RID: 4417
            public string text;

            // Token: 0x04001142 RID: 4418
            public float time;

            // Token: 0x04001143 RID: 4419
            public Color color;
        }
    }
}

