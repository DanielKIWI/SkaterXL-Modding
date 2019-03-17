using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using GUILayoutLib;

namespace DebugGUI {
    class DebugHierarchyGUI : MonoBehaviour {
        public static DebugHierarchyGUI Instance { get; private set; }
        static float pixelPerIndentLevel = 20;
        private List<GameObject> objectsWithExtendedHierarchy;
        private Rect hierarchyWindowRect = new Rect(10, 10, 300, 800);
        private Rect inspectorWindowRect = new Rect(Screen.width - 310, 10, 300, 800);
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
        public GameObject SelectedGameObject;
        Vector2 hierarchyScrollPosition;

        LineDrawer lineDrawer;
        LineDrawer lineDrawer2;
        LineDrawer lineDrawer3;

        public void Awake() {
            lineDrawer = new LineDrawer(0.02f);
            lineDrawer2 = new LineDrawer(0.02f);
            lineDrawer3 = new LineDrawer(0.02f);
            objectsWithExtendedHierarchy = new List<GameObject>();
            Instance = this;
            FilterChanged();
        }

        public void Update() {
            if (Main.guiVisible && SelectedGameObject != null) {
                lineDrawer.DrawLineInGameView(SelectedGameObject.transform.position, SelectedGameObject.transform.right, Color.red);
                lineDrawer.DrawLineInGameView(SelectedGameObject.transform.position, SelectedGameObject.transform.up, Color.green);
                lineDrawer.DrawLineInGameView(SelectedGameObject.transform.position, SelectedGameObject.transform.forward, Color.blue);
            }
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
            hierarchyWindowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), hierarchyWindowRect, HierarchyWindow, "Hierarchy");
            if (SelectedGameObject != null) {
                inspectorWindowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), inspectorWindowRect, InspectorWindow, "Inspector");
            }
        }

        public void InspectorWindow(int windowID) {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X")) {
                SelectedGameObject = null;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Transform");
            GUILayoutHelper.Vector3Label("LocalPosition", SelectedGameObject.transform.localPosition);
            GUILayoutHelper.QuaternionLabel("LocalRotation", SelectedGameObject.transform.localRotation);
            GUILayoutHelper.Vector3Label("LocalScale", SelectedGameObject.transform.localScale);
            GUILayout.Space(2);
            GUILayoutHelper.Vector3Label("Position", SelectedGameObject.transform.position);
            GUILayoutHelper.QuaternionLabel("Rotation", SelectedGameObject.transform.rotation);
            GUILayoutHelper.Vector3Label("Scale", SelectedGameObject.transform.lossyScale);
            var transformIndex = XLShredReplayEditor.ReplayManager.Instance.recorder.transformsToBeRecorded.IndexOf(SelectedGameObject.transform);
            if (transformIndex >= 0) {
                int prevIndex = XLShredReplayEditor.ReplayManager.Instance.previousFrameIndex;
                var prevFrame = XLShredReplayEditor.ReplayManager.Instance.recorder.recordedFrames[prevIndex];
                var nextFrame = XLShredReplayEditor.ReplayManager.Instance.recorder.recordedFrames[prevIndex + 1];
                var prevFrameTransformInfo = prevFrame.transformInfos[transformIndex];
                var nextFrameTransformInfo = nextFrame.transformInfos[transformIndex];

                GUILayout.Space(2);
                GUILayout.Label("PreviousFrame");
                GUILayoutHelper.Vector3Label("Position", prevFrameTransformInfo.position);
                GUILayoutHelper.QuaternionLabel("Rotation", prevFrameTransformInfo.rotation);
                GUILayoutHelper.Vector3Label("Scale", prevFrameTransformInfo.scale);

                GUILayout.Label("NextFrame");
                GUILayoutHelper.Vector3Label("Position", nextFrameTransformInfo.position);
                GUILayoutHelper.QuaternionLabel("Rotation", nextFrameTransformInfo.rotation);
                GUILayoutHelper.Vector3Label("Scale", nextFrameTransformInfo.scale);
            }
            GUILayout.Space(10);
            GUILayout.Label("Components");
            GUILayout.Space(5);
            foreach (Component component in SelectedGameObject.GetComponents<Component>()) {
                GUILayout.Label("  " + component.GetType().ToString()); 
            }


            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.RepeatButton("+")) {
                if (!resizingWindow)
                    StartCoroutine(ChangeInspectorWindowSize(true));
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.RepeatButton("+")) {
                if (!resizingWindow)
                    StartCoroutine(ChangeInspectorWindowSize(false));
            }
            GUILayout.EndHorizontal();
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
            hierarchyScrollPosition = GUILayout.BeginScrollView(hierarchyScrollPosition);
            foreach (GameObject go in filteredObjects) {
                DrawObjectHirarchy(go, 0);
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.RepeatButton("+")) {
                if (!resizingWindow)
                    StartCoroutine(ChangeHierarchyWindowSize(true));
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.RepeatButton("+")) {
                if (!resizingWindow)
                    StartCoroutine(ChangeHierarchyWindowSize(false));
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
            string labelText =
                (XLShredReplayEditor.ReplayManager.Instance.recorder.transformsToBeRecorded.Contains(go.transform) ? "[●Rec●] " : "") +
                (go.activeSelf ? "" : "[Inactive] ") + 
                go.name;
            if (GUILayout.Button(labelText)) {
                SelectedGameObject = go;
            }
            
            //Draw Button that shows the Transform Space
            //Sets Debug Gizmos Target to that transform
            //IDea: Display local rotation to see lagging there 

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Gizmos")) {
                SelectedGameObject = go;
            }
            GUILayout.EndHorizontal();
            

            if (extended && canExtend) {
                for (int i = 0; i < go.transform.childCount; i++) {
                    GameObject child = go.transform.GetChild(i).gameObject;
                    DrawObjectHirarchy(child, indentLevel + 1);
                }
            }
        }
        public IEnumerator ChangeHierarchyWindowSize(bool left) {
            resizingWindow = true;
            Vector2 mousGUIPos = (Vector2)Input.mousePosition;
            mousGUIPos.y = Screen.height - mousGUIPos.y;
            float xMax = hierarchyWindowRect.xMax;
            Vector2 edge = left ? new Vector2(hierarchyWindowRect.xMin, hierarchyWindowRect.yMax) : hierarchyWindowRect.max;
            var offset = edge - mousGUIPos;
            yield return null;
            while (Input.GetKey(KeyCode.Mouse0)) {
                mousGUIPos = (Vector2)Input.mousePosition;
                mousGUIPos.y = Screen.height - mousGUIPos.y;
                mousGUIPos += offset;
                if (left) {
                    hierarchyWindowRect.xMin = Mathf.Min(xMax - 200, mousGUIPos.x);
                    hierarchyWindowRect.xMax = xMax;
                } else {
                    hierarchyWindowRect.width = Mathf.Max(200, mousGUIPos.x - hierarchyWindowRect.x);
                }
                hierarchyWindowRect.height = Mathf.Max(200, mousGUIPos.y - hierarchyWindowRect.y);
                yield return null;
            }
            resizingWindow = false;
        }
        public IEnumerator ChangeInspectorWindowSize(bool left) {
            resizingWindow = true;
            Vector2 mousGUIPos = (Vector2)Input.mousePosition;
            mousGUIPos.y = Screen.height - mousGUIPos.y;
            float xMax = inspectorWindowRect.xMax;
            Vector2 edge = left ? new Vector2(inspectorWindowRect.xMin, inspectorWindowRect.yMax) : inspectorWindowRect.max;
            var offset = edge - mousGUIPos;
            yield return null;
            while (Input.GetKey(KeyCode.Mouse0)) {
                mousGUIPos = (Vector2)Input.mousePosition;
                mousGUIPos.y = Screen.height - mousGUIPos.y;
                mousGUIPos += offset;
                if (left) {
                    inspectorWindowRect.xMin = Mathf.Min(xMax - 200, mousGUIPos.x);
                    inspectorWindowRect.xMax = xMax;
                } else {
                    inspectorWindowRect.width = Mathf.Max(200, mousGUIPos.x - inspectorWindowRect.x);
                }
                inspectorWindowRect.height = Mathf.Max(200, mousGUIPos.y - inspectorWindowRect.y);
                yield return null;
            }
            resizingWindow = false;
        }
    }
}
