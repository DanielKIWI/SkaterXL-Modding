using System;
using UnityEngine;

// Token: 0x0200021C RID: 540
public class TransformInfo
{
    // Token: 0x060016BB RID: 5819 RVA: 0x000115CD File Offset: 0x0000F7CD
    public TransformInfo(Transform t)
    {
        this.position = t.position;
        this.rotation = t.rotation;
        this.scale = t.localScale;
    }

    // Token: 0x060016BC RID: 5820 RVA: 0x000115F9 File Offset: 0x0000F7F9
    public void ApplyTo(Transform t)
    {
        t.position = this.position;
        t.rotation = this.rotation;
        t.localScale = this.scale;
    }

    // Token: 0x060016BD RID: 5821 RVA: 0x0001161F File Offset: 0x0000F81F
    private TransformInfo(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        this.position = pos;
        this.rotation = rot;
        this.scale = scale;
    }

    // Token: 0x060016BE RID: 5822 RVA: 0x0001163C File Offset: 0x0000F83C
    public static TransformInfo Lerp(TransformInfo a, TransformInfo b, float t)
    {
        return new TransformInfo(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), Vector3.Lerp(a.position, b.position, t));
    }

    // Token: 0x040010D2 RID: 4306
    public Vector3 position;

    // Token: 0x040010D3 RID: 4307
    public Quaternion rotation;

    // Token: 0x040010D4 RID: 4308
    public Vector3 scale;
}
