using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugGUI {
    class DebugHierarchyGUI : MonoBehaviour {
        public static DebugHierarchyGUI Instance { get; private set; }
        static float pixelPerIndentLevel = 8;
        private List<GameObject> objectsWithExtendedHierarchy;
        private Rect windowRect = new Rect(10, 10, 300, 800);
        bool showInactiveObjects = true;
        bool resizingWindow = false;
        private string _objectNameFilter = "";
        public string objectNameFilter {
            get {
                return _objectNameFilter;
            }
            set {
                if (_objectNameFilter == value) return;
                _objectNameFilter = value;
                FilterChanged();
            }
        }
        GameObject[] filteredObjects;
        public void Awake() {
            objectsWithExtendedHierarchy = new List<GameObject>();
            Instance = this;
            FilterChanged();
        }

        void FilterChanged() {
            if (objectNameFilter != null && objectNameFilter.Length > 0) {
                filteredObjects = FindObjectsOfType<Transform>().Where(go => go.name.Contains(objectNameFilter)).Select(t => t.gameObject).ToArray();
            } else {
                var objects = FindObjectsOfType<Transform>();
                var rootObjects = objects.Where(t => t.parent == null).Select(t => t.gameObject);
                filteredObjects = rootObjects.ToArray();
            }
        }
        public void OnGUI() {
            if (!Main.guiVisible) return;
            windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), windowRect, HierarchyWindow, "Hierarchy");
        }

        public void HierarchyWindow(int windowID) {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();
            showInactiveObjects = GUILayout.Toggle(showInactiveObjects, "show inactive GameObjects");
            GUILayout.Space(8);
            GUILayout.Label("Search");
            GUILayout.BeginHorizontal();

            objectNameFilter = GUILayout.TextField(objectNameFilter);
            GUILayout.EndHorizontal();

            foreach (GameObject go in filteredObjects) {
                DrawObjectHirarchy(go, 0);
            }


            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //GUIStyle style = new GUIStyle(GUI.skin.button);
            //style.fontSize = 20;
            //style.clipping = TextClipping.Overflow;
            if (GUILayout.RepeatButton("+")) {
                if (!resizingWindow)
                    StartCoroutine(ChangeWindowSize());
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }
        public void DrawObjectHirarchy(GameObject go, int indentLevel, bool canExtend = true) {
            if (!showInactiveObjects && !go.activeSelf) return;

            var gizmosComponent = go.GetComponent<DebugGizmosComponent>();
            bool gizmosShown = gizmosComponent != null;
            bool extended = objectsWithExtendedHierarchy.Contains(go);

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentLevel * pixelPerIndentLevel);

            if (!canExtend || go.transform.childCount == 0) {
                GUILayout.Label("  -", GUILayout.Width(20));
            } else {
                if (GUILayout.Button(extended ? "v" : ">", GUILayout.Width(20))) {
                    extended = !extended;
                    if (extended) {
                        objectsWithExtendedHierarchy.Add(go);
                    } else {
                        objectsWithExtendedHierarchy.Remove(go);
                    }
                }
            }
            string labelText = (go.activeSelf ? "" : "[Inactive] ") + go.name;
            GUILayout.Label(labelText);
            GUILayout.FlexibleSpace();
            bool showGizmos = GUILayout.Toggle(gizmosShown, "G");
            GUILayout.EndHorizontal();

            if (showGizmos && !gizmosShown) {
                gizmosComponent = go.AddComponent<DebugGizmosComponent>();
            } else if (!showGizmos && gizmosShown) {
                Destroy(gizmosComponent);
            }

            if (extended && canExtend) {
                for (int i = 0; i < go.transform.childCount; i++) {
                    GameObject child = go.transform.GetChild(i).gameObject;
                    DrawObjectHirarchy(child, indentLevel + 1);
                }
            }
        }
        public IEnumerator ChangeWindowSize() {
            resizingWindow = true;
            Vector2 mousGUIPos = (Vector2)Input.mousePosition;
            mousGUIPos.y = Screen.height - mousGUIPos.y;
            var offset = windowRect.max - mousGUIPos;
            yield return null;
            while (Input.GetKey(KeyCode.Mouse0)) {
                mousGUIPos = (Vector2)Input.mousePosition;
                mousGUIPos.y = Screen.height - mousGUIPos.y;
                mousGUIPos += offset;

                windowRect.width = Mathf.Max(200, mousGUIPos.x - windowRect.x);
                windowRect.height = Mathf.Max(200, mousGUIPos.y - windowRect.y);
                yield return null;
            }
            resizingWindow = false;
        }
    }
}
