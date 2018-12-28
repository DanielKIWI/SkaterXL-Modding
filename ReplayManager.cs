using System;
using UnityEngine;

// Token: 0x0200021A RID: 538
public class ReplayManager : MonoBehaviour
{
    // Token: 0x060016A8 RID: 5800
    public void Awake()
    {
        ReplayManager._instance = this;
        PromptController.Instance.menuthing.enabled = false;
        this.recorder = base.gameObject.AddComponent<ReplayRecorder>();
        this.replayCamera = base.gameObject.AddComponent<ReplayCameraController>();
        this.replayCamera.enabled = false;
        this.guiColor = Color.white;
        this.fontLarge = new GUIStyle();
        this.fontMed = new GUIStyle();
        this.fontSmall = new GUIStyle();
        this.fontLarge.fontSize = 32;
        this.fontLarge.normal.textColor = Color.white;
        this.fontMed.fontSize = 14;
        this.fontMed.normal.textColor = this.guiColor;
        this.fontSmall.fontSize = 12;
        this.fontSmall.normal.textColor = this.guiColor;
        this.isEditorActive = false;
    }

    // Token: 0x060016A9 RID: 5801
    public void Update()
    {
        this.CheckInput();
        if (this.isEditorActive && this.playBackTimeScale != 0f)
        {
            if (this.playbackTime <= 0f && this.playBackTimeScale < 0f)
            {
                this.playBackTimeScale = 0f;
                this.playbackTime = 0f;
            }
            else if (this.playbackTime >= this.recorder.recordedTime && this.playBackTimeScale > 0f)
            {
                this.playBackTimeScale = 0f;
                this.playbackTime = this.recorder.recordedTime;
            }
            int frameIndex = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrame);
            this.recorder.ApplyRecordedFrame(frameIndex);
            this.previousFrame = frameIndex;
            float num = this.playbackTime;
            this.playbackTime += Time.unscaledDeltaTime * this.playBackTimeScale;
        }
    }

    // Token: 0x060016AA RID: 5802
    private void CheckInput()
    {
        if (!this.isEditorActive && PlayerController.Instance.inputController.player.GetButtonDown("Start"))
        {
            this.StartReplayEditor();
        }
        if (this.isEditorActive)
        {
            if (PlayerController.Instance.inputController.player.GetButtonDown("Select"))
            {
                this.guiHidden = !this.guiHidden;
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("B") || Input.GetKeyDown(KeyCode.Escape))
            {
                this.ExitReplayEditor();
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("A"))
            {
                if (this.playBackTimeScale == 0f)
                {
                    this.playBackTimeScale = 1f;
                }
                else
                {
                    this.playBackTimeScale = 0f;
                }
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("LB"))
            {
                this.SetPlaybackTime(this.playbackTime - 5f);
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("RB"))
            {
                this.SetPlaybackTime(this.playbackTime + 5f);
            }
            float f = PlayerController.Instance.inputController.player.GetAxis("RT") - PlayerController.Instance.inputController.player.GetAxis("LT");
            if ((double)Mathf.Abs(f) > 0.001)
            {
                this.playBackTimeScale = f;
            }
        }
    }

    // Token: 0x1700059E RID: 1438
    // (get) Token: 0x060016AB RID: 5803
    public static ReplayManager Instance
    {
        get
        {
            return ReplayManager._instance;
        }
    }

    // Token: 0x060016AC RID: 5804
    public void OnGUI()
    {
        try
        {
            if (this.isEditorActive)
            {
                if (GUI.Button(new Rect((float)Screen.width / 2f - 10f, (float)Screen.height - 20f, 20f, 20f), this.guiHidden ? "▲" : "▼"))
                {
                    this.guiHidden = !this.guiHidden;
                }
                if (!this.guiHidden)
                {
                    GUIStyle guistyle = new GUIStyle(GUI.skin.horizontalSlider);
                    int num = Screen.width - 40;
                    int num2 = Screen.height - 20 - 50;
                    float recordedTime = this.recorder.recordedTime;
                    Rect position = new Rect(20f, (float)num2, (float)num, 50f);
                    float num3 = GUI.HorizontalSlider(position, this.playbackTime, this.recorder.startTime, recordedTime, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb);
                    num2 += 50;
                    if (this.playbackTime != 0f && Mathf.Abs(num3 - this.playbackTime) > recordedTime / 200f)
                    {
                        this.SetPlaybackTime(num3);
                    }
                    int horizontal = guistyle.border.horizontal;
                    int horizontal2 = guistyle.margin.horizontal;
                    int horizontal3 = guistyle.padding.horizontal;
                    foreach (ReplayCameraController.KeyStone keyStone in this.replayCamera.keyStones)
                    {
                        float t = keyStone.time / recordedTime;
                        float xMin = position.xMin;
                        float xMax = position.xMax;
                        if (GUI.Button(new Rect(Mathf.Lerp(0f, recordedTime, t), (float)(num2 - 6), 12f, 18f), string.Empty))
                        {
                            this.playbackTime = keyStone.time;
                        }
                    }
                    Rect position2 = new Rect(20f, (float)num2 - 30f, 200f, 20f);
                    GUI.Box(position2, "");
                    GUI.Label(position2, string.Concat(new object[]
                    {
                        this.playbackTime.ToString("0.#"),
                        "s / ",
                        this.recorder.recordedTime.ToString("0.#"),
                        "s     ",
                        this.playBackTimeScale.ToString("0.#"),
                        "s/s"
                    }), this.fontMed);
                    float num4 = 40f;
                    float num5 = 40f;
                    float num6 = this.playBackTimeScale;
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 5f * num4 / 2f, (float)num2 - num5 - 10f, num4, num5), "◀◀"))
                    {
                        this.playBackTimeScale = -2f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 3f * num4 / 2f, (float)num2 - num5 - 10f, num4, num5), "◀"))
                    {
                        this.playBackTimeScale = -1f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 1f * num4 / 2f, (float)num2 - num5 - 10f, num4, num5), "▮▮"))
                    {
                        this.playBackTimeScale = 0f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 1f * num4 / 2f, (float)num2 - num5 - 10f, num4, num5), "▶"))
                    {
                        this.playBackTimeScale = 1f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 3f * num4 / 2f, (float)num2 - num5 - 10f, num4, num5), "▶▶"))
                    {
                        this.playBackTimeScale = 2f;
                    }
                }
            }
        }
        catch (Exception e)
        {
            GUIConsole.LogException(e);
        }
    }

    // Token: 0x060016AD RID: 5805
    public void SetPlaybackTime(float t)
    {
        this.playbackTime = Mathf.Clamp(t, 0f, this.recorder.recordedTime);
        int frameIndex = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrame);
        this.recorder.ApplyRecordedFrame(frameIndex);
        this.previousFrame = frameIndex;
    }

    // Token: 0x060016AE RID: 5806
    public void StartReplayEditor()
    {
        Cursor.visible = true;
        this.recorder.StopRecording();
        this.isEditorActive = true;
        this.playBackTimeScale = 1f;
        Time.timeScale = 0f;
        this.previousFrame = this.recorder.frameCount - 1;
        this.playbackTime = this.recorder.recordedTime;
        PlayerController.Instance.animationController.skaterAnim.enabled = false;
        PlayerController.Instance.cameraController.enabled = false;
        InputController.Instance.enabled = false;
        this.replayCamera.OnStartReplayEditor();
    }

    // Token: 0x060016AF RID: 5807
    public void ExitReplayEditor()
    {
        Cursor.visible = false;
        if (!this.isEditorActive)
        {
            return;
        }
        this.recorder.ApplyRecordedFrame(this.recorder.frameCount - 1);
        this.isEditorActive = false;
        Time.timeScale = 1f;
        this.recorder.isRecording = true;
        PlayerController.Instance.animationController.skaterAnim.enabled = true;
        PlayerController.Instance.cameraController.enabled = true;
        InputController.Instance.enabled = true;
        this.replayCamera.OnExitReplayEditor();
    }

    // Token: 0x040010BB RID: 4283
    private bool showKeys = true;

    // Token: 0x040010BC RID: 4284
    private GUIStyle fontLarge;

    // Token: 0x040010BD RID: 4285
    private GUIStyle fontMed;

    // Token: 0x040010BE RID: 4286
    private GUIStyle fontSmall;

    // Token: 0x040010BF RID: 4287
    private Color guiColor;

    // Token: 0x040010C0 RID: 4288
    private static ReplayManager _instance;

    // Token: 0x040010C1 RID: 4289
    public ReplayRecorder recorder;

    // Token: 0x040010C2 RID: 4290
    public float playbackTime;

    // Token: 0x040010C3 RID: 4291
    public float playBackTimeScale;

    // Token: 0x040010C4 RID: 4292
    public int previousFrame;

    // Token: 0x040010C5 RID: 4293
    private bool isEditorActive;

    // Token: 0x040010C6 RID: 4294
    public bool guiHidden;

    // Token: 0x040010C7 RID: 4295
    private ReplayCameraController replayCamera;
}
