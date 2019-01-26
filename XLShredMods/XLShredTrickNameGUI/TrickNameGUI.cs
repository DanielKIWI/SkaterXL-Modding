using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XLShredTrickNameGUI {
    class TrickNameGUI: MonoBehaviour {
        private static TrickNameGUI _instance;
        public static TrickNameGUI Instance {
            get { return _instance; }
        }
        private GUIStyle labelStyle;
        void Awake() {
            if (_instance != null)
                Debug.LogWarning("Multiple instances of TrickNameGUI!!!");
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public void OnGUI() {
            if (labelStyle == null) {
                labelStyle = new GUIStyle(GUI.skin.label) {
                    fontSize = 40
                };
            }
            string grindString = PlayerController.Instance.boardController.triggerManager.grindDetection.grindType.ToString();

            string trickString = "Grind: " + grindString; //TODO add trick in, trick out
            GUIContent content = new GUIContent(trickString);
            Vector2 size = labelStyle.CalcSize(content);
            Rect frame = new Rect {
                x = Screen.width - size.x - 10f,
                y = Screen.height - size.y - 10f,
                width = size.x,
                height = size.y
            };
            GUI.Label(frame, trickString, labelStyle);
        }

        //Will be called from PlayerState function postfixes
        public void OnPop() {

        }
        public void OnEnterGrind() {

        }
        public void OnExitGrind() {

        }
        public void OnLanding() {

        }
    }
}
