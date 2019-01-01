using System;
using UnityEngine;

namespace XLShredReplayEditor {



    [Serializable]
    public class ReplaySkaterState {

        public void ApplyTo(Transform[] transforms) {
            for (int i = 0; i < transforms.Length; i++) {
                this.transformInfos[i].ApplyTo(transforms[i]);
            }
        }


        public ReplaySkaterState(Transform[] transforms, float time) {
            this.time = time;
            this.transformInfos = new TransformInfo[transforms.Length];
            for (int i = 0; i < transforms.Length; i++) {
                this.transformInfos[i] = new TransformInfo(transforms[i]);
            }
        }


        public ReplaySkaterState(TransformInfo[] transforms, float time) {
            this.time = time;
            this.transformInfos = transforms;
        }


        public static ReplaySkaterState Lerp(ReplaySkaterState a, ReplaySkaterState b, float time) {
            if (time <= a.time) {
                return a;
            }
            if (time >= b.time) {
                return b;
            }
            return new ReplaySkaterState(a, b, time);
        }


        public ReplaySkaterState(ReplaySkaterState a, ReplaySkaterState b, float time) {
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
