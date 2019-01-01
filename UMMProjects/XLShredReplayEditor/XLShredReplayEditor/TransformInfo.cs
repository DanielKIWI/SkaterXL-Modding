using System;
using UnityEngine;

namespace XLShredReplayEditor {
    
    // Token: 0x0200021D RID: 541
    public class TransformInfo {
        // Token: 0x060016C3 RID: 5827 RVA: 0x000115DB File Offset: 0x0000F7DB
        public TransformInfo(Transform t) {
            this.position = t.position;
            this.rotation = t.rotation;
            this.scale = t.localScale;
        }

        // Token: 0x060016C4 RID: 5828 RVA: 0x00011607 File Offset: 0x0000F807
        public void ApplyTo(Transform t) {
            t.position = this.position;
            t.rotation = this.rotation;
            t.localScale = this.scale;
        }

        // Token: 0x060016C5 RID: 5829 RVA: 0x0001162D File Offset: 0x0000F82D
        private TransformInfo(Vector3 pos, Quaternion rot, Vector3 scale) {
            this.position = pos;
            this.rotation = rot;
            this.scale = scale;
        }

        // Token: 0x060016C6 RID: 5830 RVA: 0x0001164A File Offset: 0x0000F84A
        public static TransformInfo Lerp(TransformInfo a, TransformInfo b, float t) {
            return new TransformInfo(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), Vector3.Lerp(a.scale, b.scale, t));
        }

        // Token: 0x040010D8 RID: 4312
        public Vector3 position;

        // Token: 0x040010D9 RID: 4313
        public Quaternion rotation;

        // Token: 0x040010DA RID: 4314
        public Vector3 scale;
    }

}
