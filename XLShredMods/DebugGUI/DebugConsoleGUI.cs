using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DebugGUI {
    public class DebugConsoleGUI : MonoBehaviour, ILogHandler {

        public ILogHandler parentHandler;
        private static DebugConsoleGUI _instance;
        private GUIStyle font;
        private List<DebugConsoleGUI.Message> messages;
        private GUIStyle boxStyle;
        private bool b_guiStyled;
        private Vector2 scrollPosition;
        private Dictionary<LogType, bool> showLogType;
        public bool autoScroll;
        public float maxHeight;
        public void Awake() {
            if (_instance) {
                UnityEngine.Debug.LogError("There are more than 1 DebugConsoleGUI Object");
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            this.messages = new List<DebugConsoleGUI.Message>();
            LogType[] logTypes = Enum.GetValues(typeof(LogType)).Cast<LogType>().ToArray();
            showLogType = new Dictionary<LogType, bool>();
            foreach (var type in logTypes) {
                showLogType.Add(type, true);
            }
            UnityEngine.Debug.unityLogger.filterLogType = LogType.Log;
            UnityEngine.Debug.unityLogger.logEnabled = true;
        }
        public void OnDestroy() {
            UnityEngine.Debug.unityLogger.logEnabled = false;
            _instance = null;
        }


        public void OnGUI() {
            if (!Main.guiVisible) {
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
                //GUI.Box(boxRect, "Console", this.boxStyle);
                GUILayout.BeginArea(boxRect, "Console", boxStyle);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
                lock (messages) {
                    foreach (var msg in messages) {
                        this.font.normal.textColor = msg.color;
                        float messageHeight = msg.CalcHeight(font, messageWidth);
                        GUILayout.Label(msg.getGuiContent(), this.font);
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }

            GUILayout.BeginArea(new Rect(20f, boxMinY - 20f, Screen.width - 40f, 20f));
            LogType[] logTypes = Enum.GetValues(typeof(LogType)).Cast<LogType>().ToArray();
            foreach (var type in logTypes) {
                showLogType[type] = GUILayout.Toggle(showLogType[type], Enum.GetName(typeof(LogType), type));
            }
            autoScroll = GUILayout.Toggle(autoScroll, "autoScrollToBottom");
            if (GUILayout.Button("Clear")) {
                lock (messages) {
                    this.messages.Clear();
                }
            }
            GUILayout.EndArea();
        }

        public static DebugConsoleGUI Instance {
            get {
                return DebugConsoleGUI._instance;
            }
        }
        private void Update() {
            if (!Main.enabled) return;
            UnityEngine.Debug.unityLogger.logEnabled = true;
            //Remove messages older than lifetime
            messages = messages.Where(m => m.endTime > Time.unscaledTime).ToList();
            if (Input.GetKeyDown(KeyCode.F9)) {
                Main.guiVisible = !Main.guiVisible;
            }
            if (Input.GetKeyDown(KeyCode.Delete)) {
                lock (messages) {
                    this.messages.Clear();
                }
            }
        }

        private void AddMessage(LogType logType, string message) {
            lock (messages) {
                this.messages.Add(new DebugConsoleGUI.StaticMessage(logType, message));
            }
        }
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
            parentHandler.LogFormat(logType, context, format, args);
            if (!Main.enabled) return;
            if (showLogType[logType]) {
                AddMessage(logType, String.Format(format, args));
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
            AddMessage(LogType.Exception, text);
        }


        public abstract class Message {
            public float endTime;
            public Color color;
            public virtual float CalcHeight(GUIStyle style, float width) {
                return style.CalcHeight(getGuiContent(), width);
            }
            public abstract GUIContent getGuiContent();
        }

        public class StaticMessage : Message {
            private string text;
            private Color colorForLogType(LogType logType) {
                switch (logType) {
                    case LogType.Exception:
                    case LogType.Error: return Color.red;
                    case LogType.Assert: return Color.black;
                    case LogType.Warning: return Color.yellow;
                    case LogType.Log: return Color.white;
                    default: return Color.white;
                }
            }
            private float lifeTimeFactorFor(LogType logType) {
                switch (logType) {
                    case LogType.Exception: return 10f;
                    case LogType.Error: return 5f;
                    case LogType.Assert: return 2f;
                    case LogType.Warning: return 2f;
                    case LogType.Log: return 1f;
                    default: return 1f;
                }
            }

            public StaticMessage(LogType logType, string txt) {
                this.text = Enum.GetName(typeof(LogType), logType) + ": " + txt;
                this.color = colorForLogType(logType);
                this.endTime = Time.unscaledTime + Main.settings.MessageLifeTime * lifeTimeFactorFor(logType);
            }

            public StaticMessage(string txt, Color color) {
                this.text = txt;
                this.endTime = Time.unscaledTime;
                this.color = color;
            }
            public override GUIContent getGuiContent() {
                return new GUIContent(text);
            }
        }

        //public class DynamicMessage : Message {
        //    public DynamicMessage(Func<string> func) {
        //        this.textFunc = func;
        //        this.EndTime = Time.unscaledTime;
        //        this.color = Color.white;
        //    }

        //    public DynamicMessage(Func<string> func, Color color) {
        //        this.textFunc = func;
        //        this.EndTime = Time.unscaledTime;
        //        this.color = color;
        //    }
        //    public override GUIContent getGuiContent() {
        //        return new GUIContent(textFunc());
        //    }

        //    Token: 0x04001141 RID: 4417
        //    private Func<string> textFunc;
        //}

    }
    public static class MessageListExtension {
        public static float CalcHeight(this IEnumerable<DebugConsoleGUI.Message> messages, GUIStyle style, float width) {
            float height = 0f;
            foreach (DebugConsoleGUI.Message message in messages) {
                height += message.CalcHeight(style, width);
            }
            return height;
        }
    }
}

