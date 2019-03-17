using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {
    
    [Serializable]
    public class ReplayRecordedFrame {

        public void ApplyTo(List<Transform> transforms) {
            for (int i = 0; i < transforms.Count; i++) {
                this.transformInfos[i].ApplyTo(transforms[i]);
            }
        }


        public ReplayRecordedFrame(List<Transform> transforms, float time) {
            this.time = time;
            this.transformInfos = new TransformInfo[transforms.Count];
            for (int i = 0; i < transforms.Count; i++) {
                this.transformInfos[i] = new TransformInfo(transforms[i]);
            }
        }


        public ReplayRecordedFrame(TransformInfo[] transforms, float time) {
            this.time = time;
            this.transformInfos = transforms;
        }


        public static ReplayRecordedFrame Lerp(ReplayRecordedFrame a, ReplayRecordedFrame b, float time) {
            if (time <= a.time) {
                return a;
            }
            if (time >= b.time) {
                return b;
            }
            return new ReplayRecordedFrame(a, b, time);
        }


        public ReplayRecordedFrame(ReplayRecordedFrame a, ReplayRecordedFrame b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            this.time = time;
            this.transformInfos = new TransformInfo[a.transformInfos.Length];
            for (int i = 0; i < a.transformInfos.Length; i++) {
                this.transformInfos[i] = TransformInfo.Lerp(a.transformInfos[i], b.transformInfos[i], t);
            }
        }


        public TransformInfo[] transformInfos;

        public float time;
    }
}
