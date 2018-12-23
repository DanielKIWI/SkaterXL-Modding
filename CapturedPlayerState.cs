using System;
using UnityEngine;

// Token: 0x0200021D RID: 541
public class CapturedPlayerState
{
    // Token: 0x060016BD RID: 5821 RVA: 0x00071404 File Offset: 0x0006F604
    public void ApplyTo(Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            this.transformInfos[i].ApplyTo(transforms[i]);
        }
    }

    // Token: 0x060016BE RID: 5822 RVA: 0x00071448 File Offset: 0x0006F648
    public CapturedPlayerState(Transform[] transforms)
    {
        this.transformInfos = new TransformInfo[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            this.transformInfos[i] = new TransformInfo(transforms[i]);
        }
    }

    // Token: 0x040010D5 RID: 4309
    public TransformInfo[] transformInfos;
}
