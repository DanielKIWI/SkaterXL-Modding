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
        private Vector2 scrollPosition;
        private Dictionary<LogType, bool> showLogType;
        public bool autoScroll;
        public float maxHeight;
        public void Awake() {
            DontDestroyOnLoad(gameObject);
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
            float boxWidth = (float)Screen.width - 20f;
            if (this.messages.Count > 0) {
                float topMargin = 40f;
                float messageWidth = boxWidth - 20f;
                //Calculating Height of Messages
                float height = 30f + messages.CalcHeight(this.font, messageWidth);
                if (height < Screen.height - 30 - topMargin) {
                    boxMinY = (float)Screen.height - height - 10f;
                } else {
                    boxMinY = 30f;
                }
                Rect boxRect = new Rect() {
                    x = 10f,
                    yMin = boxMinY,
                    width = boxWidth,
                    yMax = Screen.height - 20f
                };
                Rect viewRect = new Rect(10f, 10f, messageWidth, height);
                GUI.Box(boxRect, "Console", this.boxStyle);

                scrollPosition = GUI.BeginScrollView(boxRect, scrollPosition, viewRect, false, true);
                float yOffset = 20f;
                foreach (DebugGUI.StaticMessage message2 in this.messages) {
                    this.font.normal.textColor = message2.color;
                    float messageHeight = message2.CalcHeight(font, messageWidth);
                    GUI.Label(new Rect(20f, yOffset, messageWidth, messageHeight), message2.getGuiContent(), this.font);
                    yOffset += messageHeight;
                }
                GUI.EndScrollView(true);
                //float 
                //    GUI.VerticalSlider()
            }
            LogType[] logTypes = Enum.GetValues(typeof(LogType)).Cast<LogType>().ToArray();
            float x = 20f;
            float w = 0f;
            foreach (var type in logTypes) {
                GUIContent c1 = new GUIContent(Enum.GetName(typeof(LogType), type));
                w = GUI.skin.toggle.CalcSize(c1).x;
                showLogType[type] = GUI.Toggle(new Rect(x, boxMinY - 20f, w, 20f), showLogType[type], c1);
                x += w;
            }
            GUIContent c2 = new GUIContent("autoScrollToBottom");
            w = GUI.skin.toggle.CalcSize(c2).x;
            autoScroll = GUI.Toggle(new Rect(x, boxMinY - 20f, w, 20f), autoScroll, c2);
            x += w;
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
            if (!Main.enabled) return;
            UnityEngine.Debug.unityLogger.logEnabled = true;
            //Remove messages older than lifetime
            foreach (StaticMessage msg in messages) {
                if (Time.unscaledTime - this.messages[0].time > Main.settings.MessageLifeTime) {
                    this.messages.RemoveAt(0);
                }
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
            this.messages.Add(new DebugGUI.StaticMessage(message, color));

            if (this.messages.Count > Main.settings.MaxLogsCount) {
                this.messages.RemoveAt(0);
            }
        }
        private Color colorForLogType(LogType type) {
            switch (type) {
                case LogType.Exception: return Color.red;
                case LogType.Error: return Color.red;
                case LogType.Assert: return Color.black;
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
            string text = "Exception: " + exception.Message;
            for (int i = 0; i < Mathf.Min(stackTrace.FrameCount, 4); i++) {
                text = text + "\n" + stackTrace.GetFrame(i).ToString();
            }
            AddMessage(text, colorForLogType(LogType.Exception));
        }


        public abstract class Message {
            public virtual float CalcHeight(GUIStyle style, float width) {
                return style.CalcHeight(getGuiContent(), width);
            }
            public abstract GUIContent getGuiContent();
            
            public float time;
            
            public Color color;
        }

        public class StaticMessage: Message {
            public StaticMessage(string txt) {
                this.text = txt;
                this.time = Time.unscaledTime;
                this.color = Color.white;
            }
            
            public StaticMessage(string txt, Color color) {
                this.text = txt;
                this.time = Time.unscaledTime;
                this.color = color;
            }
            public override GUIContent getGuiContent() {
                return new GUIContent(text);
            }
            private string text;
        }

        public class DynamicMessage : Message {
            public DynamicMessage(Func<string> func) {
                this.textFunc = func;
                this.time = Time.unscaledTime;
                this.color = Color.white;
            }
            
            public DynamicMessage(Func<string> func, Color color) {
                this.textFunc = func;
                this.time = Time.unscaledTime;
                this.color = color;
            }
            public override GUIContent getGuiContent() {
                return new GUIContent(textFunc());
            }

            // Token: 0x04001141 RID: 4417
            private Func<string> textFunc;
        }

    }
    public static class MessageListExtension {
        public static float CalcHeight(this IEnumerable<DebugGUI.Message> messages, GUIStyle style, float width) {
            float height = 0f;
            foreach (DebugGUI.Message message in messages) {
                height += message.CalcHeight(style, width);
            }
            return height;
        }
    }
}

