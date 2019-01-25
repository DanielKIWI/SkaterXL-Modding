using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLMultiplayerMod {
    class TransformChangeRecorder: MonoBehaviour {
        Vector3 lastLocalPos;
        Vector3 lastLocalEuler;
        Vector3 lastLocalScale;

        public struct Vector3KeyFrame {
            public Vector3 value;
            public float time;
            public Vector3KeyFrame(Vector3 v, float t) {
                value = v; time = t;
            }
        }

        List<Vector3KeyFrame> localPosKeyFrames = new List<Vector3KeyFrame>();
        List<Vector3KeyFrame> localEulerKeyFrames = new List<Vector3KeyFrame>();
        List<Vector3KeyFrame> localScaleKeyFrames = new List<Vector3KeyFrame>();

        public static float Tolerance = 0.1f;
        public void Update() {
            if ((lastLocalPos - transform.localPosition).magnitude > Tolerance) {
                localPosKeyFrames.Add(new Vector3KeyFrame(transform.localPosition, Time.time));
                lastLocalPos = transform.localPosition;
            }
            if ((lastLocalEuler - transform.localEulerAngles).magnitude > Tolerance) {
                localEulerKeyFrames.Add(new Vector3KeyFrame(transform.localEulerAngles, Time.time));
                lastLocalEuler = transform.localEulerAngles;
            }
            if ((lastLocalScale - transform.localScale).magnitude > Tolerance) {
                localScaleKeyFrames.Add(new Vector3KeyFrame(transform.localScale, Time.time));
                lastLocalScale = transform.localScale;
            }
        }
    }
}
