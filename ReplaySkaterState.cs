using System;
using UnityEngine;

// Token: 0x0200021D RID: 541
public class ReplaySkaterState
{
    // Token: 0x060016BF RID: 5823
    public void ApplyTo(Transform[] transforms)
    {
        if (transforms.Length != this.transformInfos.Length)
        {
            GUIConsole.LogError("Not matching Transforms Length in CapturedPlayerState.ApplyTo");
        }
        for (int i = 0; i < transforms.Length; i++)
        {
            this.transformInfos[i].ApplyTo(transforms[i]);
        }
    }

    // Token: 0x060016C1 RID: 5825
    public ReplaySkaterState(Transform[] transforms, float time)
    {
        this.time = time;
        this.transformInfos = new TransformInfo[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            this.transformInfos[i] = new TransformInfo(transforms[i]);
        }
    }

    // Token: 0x040010D5 RID: 4309
    public TransformInfo[] transformInfos;

    // Token: 0x040010D6 RID: 4310
    public float time;
}
