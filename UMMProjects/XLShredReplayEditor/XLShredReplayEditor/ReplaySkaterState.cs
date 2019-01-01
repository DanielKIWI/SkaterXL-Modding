using System;
using UnityEngine;

namespace XLShredReplayEditor {


    // Token: 0x0200021C RID: 540
    [Serializable]
    public class ReplaySkaterState {
        // Token: 0x060016BE RID: 5822 RVA: 0x00071748 File Offset: 0x0006F948
        public void ApplyTo(Transform[] transforms) {
            for (int i = 0; i < transforms.Length; i++) {
                this.transformInfos[i].ApplyTo(transforms[i]);
            }
        }

        // Token: 0x060016BF RID: 5823 RVA: 0x00071774 File Offset: 0x0006F974
        public ReplaySkaterState(Transform[] transforms, float time) {
            this.time = time;
            this.transformInfos = new TransformInfo[transforms.Length];
            for (int i = 0; i < transforms.Length; i++) {
                this.transformInfos[i] = new TransformInfo(transforms[i]);
            }
        }

        // Token: 0x060016C0 RID: 5824 RVA: 0x000115A5 File Offset: 0x0000F7A5
        public ReplaySkaterState(TransformInfo[] transforms, float time) {
            this.time = time;
            this.transformInfos = transforms;
        }

        // Token: 0x060016C1 RID: 5825 RVA: 0x000115BB File Offset: 0x0000F7BB
        public static ReplaySkaterState Lerp(ReplaySkaterState a, ReplaySkaterState b, float time) {
            if (time <= a.time) {
                return a;
            }
            if (time >= b.time) {
                return b;
            }
            return new ReplaySkaterState(a, b, time);
        }

        // Token: 0x060016C2 RID: 5826 RVA: 0x000717BC File Offset: 0x0006F9BC
        public ReplaySkaterState(ReplaySkaterState a, ReplaySkaterState b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            this.time = time;
            this.transformInfos = new TransformInfo[a.transformInfos.Length];
            for (int i = 0; i < a.transformInfos.Length; i++) {
                this.transformInfos[i] = TransformInfo.Lerp(a.transformInfos[i], b.transformInfos[i], t);
            }
        }

        // Token: 0x040010D6 RID: 4310
        public TransformInfo[] transformInfos;

        // Token: 0x040010D7 RID: 4311
        public float time;
    }

}
