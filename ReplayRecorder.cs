using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200021B RID: 539
public class ReplayRecorder : MonoBehaviour
{
    // Token: 0x060016B1 RID: 5809 RVA: 0x0007101C File Offset: 0x0006F21C
    public void Start()
    {
        List<Transform> list = new List<Transform>(PlayerController.Instance.respawn.getSpawn);
        foreach (object obj in Enum.GetValues(typeof(HumanBodyBones)))
        {
            HumanBodyBones humanBodyBones = (HumanBodyBones)obj;
            if (humanBodyBones >= HumanBodyBones.Hips && humanBodyBones < HumanBodyBones.LastBone)
            {
                Transform boneTransform = PlayerController.Instance.animationController.skaterAnim.GetBoneTransform(humanBodyBones);
                if (!(boneTransform == null))
                {
                    list.Add(boneTransform);
                }
            }
        }
        this.transformsToBeRecorded = list.ToArray();
        this.recSkin = new GUIStyle();
        this.recSkin.fontSize = 20;
        this.recSkin.normal.textColor = Color.red;
        this.recordedFrames = new List<ReplaySkaterState>();
        this.MaxRecordedTime = 300f;
        this.startTime = 0f;
        this.StartRecording();
    }

    // Token: 0x060016B2 RID: 5810 RVA: 0x00011506 File Offset: 0x0000F706
    public void StartRecording()
    {
        if (this.isRecording)
        {
            return;
        }
        this.isRecording = true;
        this.ClearRecording();
        this.recordedTime = 0f;
        this.RecordFrame();
    }

    // Token: 0x060016B3 RID: 5811 RVA: 0x0001152F File Offset: 0x0000F72F
    public void StopRecording()
    {
        if (!this.isRecording)
        {
            return;
        }
        this.isRecording = false;
    }

    // Token: 0x060016B4 RID: 5812 RVA: 0x00011541 File Offset: 0x0000F741
    private void ClearRecording()
    {
        this.recordedFrames.Clear();
        this.startTime = this.recordedTime;
    }

    // Token: 0x060016B5 RID: 5813 RVA: 0x00071120 File Offset: 0x0006F320
    public void Update()
    {
        if (!this.isRecording)
        {
            return;
        }
        this.recordedTime += Time.deltaTime;
        this.RecordFrame();
        if (this.recordedTime > this.MaxRecordedTime)
        {
            this.startTime = this.recordedTime - this.MaxRecordedTime;
            while (this.recordedFrames.Count > 0 && this.recordedFrames[0].time < this.startTime)
            {
                this.recordedFrames.RemoveAt(0);
            }
        }
    }

    // Token: 0x1700059F RID: 1439
    // (get) Token: 0x060016B6 RID: 5814 RVA: 0x0001155A File Offset: 0x0000F75A
    public int frameCount
    {
        get
        {
            return this.recordedFrames.Count;
        }
    }

    // Token: 0x060016B7 RID: 5815 RVA: 0x00011567 File Offset: 0x0000F767
    public void ApplyRecordedFrame(int frame)
    {
        this.recordedFrames[frame].ApplyTo(this.transformsToBeRecorded);
    }

    // Token: 0x060016B8 RID: 5816 RVA: 0x00011580 File Offset: 0x0000F780
    private void RecordFrame()
    {
        this.recordedFrames.Add(new ReplaySkaterState(this.transformsToBeRecorded, this.recordedTime));
    }

    // Token: 0x060016B9 RID: 5817
    public void OnGUI()
    {
        if (this.isRecording)
        {
            string txt = "● Rec";
            Vector2 size = this.recSkin.CalcSize(new GUIContent(txt));
            GUI.Label(new Rect((float)Screen.width - size.x - 10f, 10f, size.x, size.y), txt, this.recSkin);
        }
    }

    // Token: 0x060016BA RID: 5818 RVA: 0x000711A4 File Offset: 0x0006F3A4
    public int GetFrameIndex(float time, int lastFrame = 0)
    {
        if (lastFrame < 0)
        {
            lastFrame = 0;
        }
        if (lastFrame >= this.recordedFrames.Count)
        {
            lastFrame = this.recordedFrames.Count - 1;
        }
        int num = lastFrame;
        if (time < this.recordedFrames[lastFrame].time)
        {
            while (num > 0 && this.recordedFrames[num - 1].time > time)
            {
                num--;
            }
            return num;
        }
        while (num < this.frameCount - 1 && this.recordedFrames[num + 1].time < time)
        {
            num++;
        }
        return num;
    }

    // Token: 0x040010C8 RID: 4296
    public Transform[] transformsToBeRecorded;

    // Token: 0x040010C9 RID: 4297
    public bool isRecording;

    // Token: 0x040010CA RID: 4298
    public List<ReplaySkaterState> recordedFrames;

    // Token: 0x040010CB RID: 4299
    public float recordedTime;

    // Token: 0x040010CC RID: 4300
    private GUIStyle recSkin;

    // Token: 0x040010CD RID: 4301
    public float startTime;

    // Token: 0x040010CE RID: 4302
    public float MaxRecordedTime;
}
