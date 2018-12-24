using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000221 RID: 545
public class ReplayRecorder : MonoBehaviour
{
    // Token: 0x060016D7 RID: 5847
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
        this.MaxRecordedFrames = int.MaxValue;
        this.StartRecording();
    }

    // Token: 0x060016D8 RID: 5848
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

    // Token: 0x060016D9 RID: 5849
    public void StopRecording()
    {
        if (!this.isRecording)
        {
            return;
        }
        this.isRecording = false;
    }

    // Token: 0x060016DA RID: 5850
    private void ClearRecording()
    {
        for (int i = 0; i < 8; i++)
        {
            this.recordedFrames.Clear();
        }
    }

    // Token: 0x060016DB RID: 5851
    public void Update()
    {
        if (!this.isRecording)
        {
            return;
        }
        this.recordedTime += Time.deltaTime;
        this.RecordFrame();
        if (this.recordedFrames.Count > this.MaxRecordedFrames)
        {
            this.recordedFrames.RemoveAt(0);
        }
    }

    // Token: 0x170005A2 RID: 1442
    // (get) Token: 0x060016DC RID: 5852
    public int frameCount
    {
        get
        {
            return this.recordedFrames.Count;
        }
    }

    // Token: 0x060016DD RID: 5853
    public void ApplyRecordedFrame(int frame)
    {
        this.recordedFrames[frame].ApplyTo(this.transformsToBeRecorded);
    }

    // Token: 0x060016DE RID: 5854
    private void RecordFrame()
    {
        this.recordedFrames.Add(new ReplaySkaterState(this.transformsToBeRecorded, this.recordedTime));
    }

    // Token: 0x060016DF RID: 5855
    public void OnGUI()
    {
        if (this.isRecording)
        {
            GUI.Label(new Rect((float)Screen.width - 40f, 10f, 30f, 30f), "● Rec", this.recSkin);
        }
    }

    // Token: 0x060016E0 RID: 5856
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

    // Token: 0x040010EB RID: 4331
    public Transform[] transformsToBeRecorded;

    // Token: 0x040010EC RID: 4332
    public bool isRecording;

    // Token: 0x040010ED RID: 4333
    public List<ReplaySkaterState> recordedFrames;

    // Token: 0x040010EE RID: 4334
    public int MaxRecordedFrames;

    // Token: 0x040010EF RID: 4335
    public float recordedTime;

    // Token: 0x040010F0 RID: 4336
    private GUIStyle recSkin;
}
