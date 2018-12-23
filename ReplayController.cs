using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000221 RID: 545
public class ReplayController : MonoBehaviour
{
    // Token: 0x060016CC RID: 5836
    public void Awake()
    {
        ReplayController._instance = this;
        this.capturedFrames = new List<CapturedPlayerState__0>();
        this.MaxRecordedFrames = 2000;
        this.playBackFramesPerFrame = 1;
        this.guiColor = default(Color);
        ColorUtility.TryParseHtmlString("#00FF00", out this.guiColor);
        this.fontLarge = new GUIStyle();
        this.fontMed = new GUIStyle();
        this.fontSmall = new GUIStyle();
        this.fontLarge.fontSize = 32;
        this.fontLarge.normal.textColor = Color.white;
        this.fontMed.fontSize = 14;
        this.fontMed.normal.textColor = this.guiColor;
        this.fontSmall.fontSize = 12;
        this.fontSmall.normal.textColor = this.guiColor;
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
    }

    // Token: 0x060016CE RID: 5838
    public void Update()
    {
        this.CheckInput();
        if (Input.GetKey(KeyCode.F5))
        {
            return;
        }
        if (this.isRecording)
        {
            this.CapturePlayerState();
        }
        if (this.isPlayback && this.playBackFramesPerFrame != 0)
        {
            if (this.playBackFramesPerFrame > 0 && this.playBackFrame >= this.capturedFrames.Count - 1)
            {
                this.playBackFramesPerFrame = 0;
            }
            else if (this.playBackFramesPerFrame < 0 && this.playBackFrame <= 0)
            {
                this.playBackFramesPerFrame = 0;
            }
            this.capturedFrames[this.playBackFrame].ApplyTo(this.transformsToBeRecorded);
            this.playBackFrame += this.playBackFramesPerFrame;
        }
    }

    // Token: 0x060016CF RID: 5839
    public void StartRecording()
    {
        if (this.isPlayback || this.isRecording)
        {
            return;
        }
        this.isRecording = true;
        this.ClearRecording();
    }

    // Token: 0x060016D0 RID: 5840
    public void StopRecording()
    {
        if (this.isPlayback || !this.isRecording)
        {
            return;
        }
        this.isRecording = false;
    }

    // Token: 0x060016D1 RID: 5841
    private void ClearRecording()
    {
        for (int i = 0; i < 8; i++)
        {
            this.capturedFrames.Clear();
        }
    }

    // Token: 0x060016D2 RID: 5842
    private void CapturePlayerState()
    {
        this.capturedFrames.Add(new CapturedPlayerState__0(this.transformsToBeRecorded));
        if (this.capturedFrames.Count > 10000)
        {
            this.capturedFrames.RemoveAt(0);
        }
    }

    // Token: 0x060016D3 RID: 5843
    private void OnGUI()
    {
        GUI.color = this.guiColor;
        float num = 20f;
        float num2 = 300f;
        float num3 = (float)Screen.width - num2 - 20f;
        float num4 = 30f;
        if (this.isRecording)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "● Rec", this.fontSmall);
            GUI.color = this.guiColor;
            num4 += 20f;
        }
        else if (this.isPlayback)
        {
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "Playback current frame: " + this.playBackFrame, this.fontSmall);
            num4 += 20f;
            GUI.Slider(new Rect(num3, num + num4, num2, 40f), (float)this.playBackFrame, (float)this.capturedFrames.Count, 0f, (float)(this.capturedFrames.Count - 1), this.fontSmall, this.fontSmall, true, 1);
            num4 += 20f;
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "Playback speed: " + this.playBackFramesPerFrame, this.fontSmall);
            num4 += 20f;
        }
        GUI.Label(new Rect(num3, num + num4, num2, 40f), "Recorded Frames: " + this.capturedFrames.Count, this.fontSmall);
        num4 += 20f;
        if (this.showKeys)
        {
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "F1 - Start/Stop Recording", this.fontSmall);
            num4 += 20f;
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "F2 - Start/Stop Playback", this.fontSmall);
            num4 += 20f;
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "F3 - Jump 60 Frames back", this.fontSmall);
            num4 += 20f;
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "F4 - Jump 60 Frames ahead", this.fontSmall);
            num4 += 20f;
            GUI.Label(new Rect(num3, num + num4, num2, 40f), "F5 - Reverse Time (while Pressing", this.fontSmall);
            num4 += 20f;
        }
        GUI.Box(new Rect(num3 - 10f, num, num2 + 20f, num4 + 10f), "ReplayMenu");
    }

    // Token: 0x060016D4 RID: 5844
    public void StartPlayback(int frame = 0)
    {
        if (this.isPlayback)
        {
            return;
        }
        this.isRecording = false;
        this.isPlayback = true;
        PlayerController.Instance.animationController.skaterAnim.enabled = false;
        Time.timeScale = 0f;
        this.playBackFrame = frame;
    }

    // Token: 0x060016D5 RID: 5845
    private void CheckInput()
    {
        if (!this.isPlayback && Input.GetKeyDown(KeyCode.F1))
        {
            if (this.isRecording)
            {
                this.StopRecording();
            }
            else
            {
                this.StartRecording();
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (this.isPlayback)
            {
                this.StopPlayback(true, 1);
            }
            else
            {
                this.StartPlayback(0);
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            this.playBackFrame = Mathf.Max(0, this.playBackFrame - 60);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            this.playBackFrame = Mathf.Min(this.capturedFrames.Count - 1, this.playBackFrame + 60);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            this.playBackFrame--;
        }
    }

    // Token: 0x170005A1 RID: 1441
    // (get) Token: 0x060016D6 RID: 5846
    public static ReplayController Instance
    {
        get
        {
            return ReplayController._instance;
        }
    }

    // Token: 0x060016D7 RID: 5847
    public void StopPlayback(bool continueCapturing = true, int framesPerFrame = 1)
    {
        this.playBackFramesPerFrame = framesPerFrame;
        if (!this.isPlayback)
        {
            return;
        }
        this.isPlayback = false;
        PlayerController.Instance.animationController.skaterAnim.enabled = true;
        Time.timeScale = 1f;
        if (continueCapturing)
        {
            this.capturedFrames.RemoveRange(this.playBackFrame + 1, this.capturedFrames.Count - this.playBackFrame - 1);
            this.isRecording = true;
        }
    }

    // Token: 0x040010DE RID: 4318
    private bool isRecording;

    // Token: 0x040010DF RID: 4319
    private bool isPlayback;

    // Token: 0x040010E0 RID: 4320
    public int playBackFrame;

    // Token: 0x040010E1 RID: 4321
    private List<CapturedPlayerState__0> capturedFrames;

    // Token: 0x040010E2 RID: 4322
    public int MaxRecordedFrames;

    // Token: 0x040010E3 RID: 4323
    public int playBackFramesPerFrame;

    // Token: 0x040010E4 RID: 4324
    private bool showKeys = true;

    // Token: 0x040010E5 RID: 4325
    private GUIStyle fontLarge;

    // Token: 0x040010E6 RID: 4326
    private GUIStyle fontMed;

    // Token: 0x040010E7 RID: 4327
    private GUIStyle fontSmall;

    // Token: 0x040010E8 RID: 4328
    private Color guiColor;

    // Token: 0x040010E9 RID: 4329
    private static ReplayController _instance;

    // Token: 0x040010EA RID: 4330
    private Transform[] transformsToBeRecorded;
}
