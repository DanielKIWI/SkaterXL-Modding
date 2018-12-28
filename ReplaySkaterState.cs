using System;
using UnityEngine;

// Token: 0x0200021C RID: 540
public class ReplaySkaterState
{
    // Token: 0x060016BC RID: 5820 RVA: 0x00071234 File Offset: 0x0006F434
    public void ApplyTo(Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            this.transformInfos[i].ApplyTo(transforms[i]);
        }
    }

    // Token: 0x060016BD RID: 5821 RVA: 0x00071260 File Offset: 0x0006F460
    public ReplaySkaterState(Transform[] transforms, float time)
    {
        this.time = time;
        this.transformInfos = new TransformInfo[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            this.transformInfos[i] = new TransformInfo(transforms[i]);
        }
    }

    // Token: 0x040010CF RID: 4303
    public TransformInfo[] transformInfos;

    // Token: 0x040010D0 RID: 4304
    public float time;
}
