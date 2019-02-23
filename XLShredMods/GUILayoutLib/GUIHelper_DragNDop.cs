using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GUILayoutLib {
    public static partial class GUIHelperSettings {
        public static float DragMoveThreshold = 3f; //pixels
        public static float DragTimeThreshold = 0.2f; //seconds
    }

    public static partial class GUIHelper {


        //public object draggedObject;
        //public float dragOverTime;

        //        if (GUI.RepeatButton(ReplaySkin.DefaultSkin.markerRect(t), ReplaySkin.DefaultSkin.markerContent, ReplaySkin.DefaultSkin.markerStyle)) {
        //            if (draggedKeyFrame == null) {
        //                draggedKeyFrame = keyStone;
        //                dragOverTime = keyStone.time;
        //                this.playbackTime = dragOverTime;
        //                StartCoroutine(DragKeyFrameUpdate());
        //}

        private static string draggedObjectID;
        private static Rect draggedRect;
        public delegate void OnDragUpdateFunc(ref Rect rect);
        public delegate void DrawDraggableAreaFunc(out bool beginDrag);

        /// <summary>
        /// Creates a draggable Button
        /// </summary>
        /// <param name="id">A unique ID.</param>
        /// <param name="rect">Button rect</param>
        /// <param name="content">Button content</param>
        /// <param name="style">Style of the RepeatButton</param>
        /// <param name="onClick">Function called if Move and Time Thresholds weren't broken before left mouse button was released.</param>
        /// <param name="onDragUpdate">Function called every Frame while dragging. Parameter: reference to the button rect. Constraints can be applied here.</param>
        /// <param name="onDrop">Function called when left mouse button is released</param>
        /// <param name="coroutineOwner">MonoBehaviour Instance that owns the coroutine. -> StopAllCoroutines will stop this if called on owner </param>
        /// <param name="cancelOnRightClick">If true the drag canceles when the right mouse button is pressed. After cancelation the ButtonRect returns to its startPosition</param>
        public static void DraggableButton(string id, Rect rect, GUIContent content, GUIStyle style, Action onClick, OnDragUpdateFunc onDragUpdate, Action<Rect> onDrop, UnityEngine.MonoBehaviour coroutineOwner, bool cancelOnRightClick = true) {

            Rect r = (draggedObjectID == id) ? draggedRect : rect;
            if (GUI.RepeatButton(r, content, style)) {
                if (draggedObjectID == null) {
                    draggedObjectID = id;
                    draggedRect = r;
                    coroutineOwner.StartCoroutine(DraggableRectUpdate(onClick, onDragUpdate, onDrop, cancelOnRightClick));
                }
            }
        }

        /// <summary>
        /// Creates a customizable Draggable Area
        /// </summary>
        /// <param name="id">A unique ID.</param>
        /// <param name="rect">Area rect</param>
        /// <param name="drawAreaContent">Function for drawing custom content. This function is called in a GUILayout Area context. Returns Whether a Drag should start.</param>
        /// <param name="onClick">Function called if Move and Time Thresholds weren't broken before left mouse button was released.</param>
        /// <param name="onDragUpdate">Function called every Frame while dragging. Parameter: reference to the area rect. Constraints can be applied here.</param>
        /// <param name="onDrop">Function called when left mouse button is released</param>
        /// <param name="coroutineOwner">MonoBehaviour Instance that owns the coroutine. -> StopAllCoroutines will stop this if called on owner!!! Do not do that!!!</param>
        /// <param name="cancelOnRightClick">If true the drag canceles when the right mouse button is pressed. After cancelation the AreaRect returns to its startPosition</param>
        public static void DraggableArea(string id, Rect rect, DrawDraggableAreaFunc drawAreaContent, Action onClick, OnDragUpdateFunc onDragUpdate, Action<Rect> onDrop, UnityEngine.MonoBehaviour coroutineOwner, bool cancelOnRightClick = true) {

            Rect r = (draggedObjectID == id) ? draggedRect : rect;
            GUILayout.BeginArea(r);
            drawAreaContent(out bool beginDrag);
            if (beginDrag && draggedObjectID == null) {
                draggedObjectID = id;
                draggedRect = r;
                coroutineOwner.StartCoroutine(DraggableRectUpdate(onClick, onDragUpdate, onDrop, cancelOnRightClick));
            }
            GUILayout.EndArea();
        }

        private static IEnumerator DraggableRectUpdate(Action onClick, OnDragUpdateFunc onDragUpdate, Action<Rect> onDrop, bool cancelOnRightClick) {
            Vector2 startPosition = Input.mousePosition;
            float startTime = Time.time;
            Vector2 offset = draggedRect.center - startPosition;
            while (((Vector2)Input.mousePosition - startPosition).magnitude < GUIHelperSettings.DragMoveThreshold && Time.time - startTime < GUIHelperSettings.DragTimeThreshold) {
                if (!Input.GetKey(KeyCode.Mouse0)) {
                    draggedObjectID = null;
                    onClick();
                    yield break;
                }
                yield return null;
            }
            while (Input.GetKey(KeyCode.Mouse0)) {
                if (cancelOnRightClick && Input.GetKeyDown(KeyCode.Mouse1)) {
                    draggedObjectID = null;
                    //onCancel();
                    yield break;
                }
                draggedRect.center = (Vector2)Input.mousePosition + offset;
                onDragUpdate(ref draggedRect);
                yield return null;
            }
            draggedObjectID = null;
            onDrop(draggedRect);
        }
    }
}
