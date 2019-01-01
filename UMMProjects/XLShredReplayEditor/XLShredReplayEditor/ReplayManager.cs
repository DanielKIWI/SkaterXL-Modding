using System;
using UnityEngine;

namespace XLShredReplayEditor {
    // Token: 0x0200021A RID: 538
    public class ReplayManager : MonoBehaviour {
        // Token: 0x060016A8 RID: 5800 RVA: 0x000114F0 File Offset: 0x0000F6F0
        public ReplayManager() {
            this.showKeys = true;
        }

        // Token: 0x060016A9 RID: 5801 RVA: 0x00070830 File Offset: 0x0006EA30
        public void Awake() {
            ReplayManager._instance = this;
            PromptController.Instance.menuthing.enabled = false;
            if (this.recorder == null) {
                this.recorder = base.gameObject.AddComponent<ReplayRecorder>();
            }
            if (this.cameraController == null) {
                this.cameraController = base.gameObject.AddComponent<ReplayCameraController>();
                this.cameraController.enabled = false;
            }
            if (this.saver == null) {
                this.saver = base.gameObject.AddComponent<ReplaySaver>();
                this.saver.enabled = false;
            }
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
            this.PlaybackTimeJumpDelta = 5f;
        }

        // Token: 0x060016AA RID: 5802 RVA: 0x00070970 File Offset: 0x0006EB70
        public void Update() {
            this.CheckInput();
            if (this.isEditorActive && this.playBackTimeScale != 0f) {
                if (this.playbackTime < this.clipStartTime && this.playBackTimeScale < 0f) {
                    this.isPlaying = false;
                    this.playbackTime = this.clipStartTime;
                } else if (this.playbackTime > this.clipEndTime && this.playBackTimeScale > 0f) {
                    this.isPlaying = false;
                    this.playbackTime = this.clipEndTime;
                }
                this.previousFrame = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrame);
                this.recorder.ApplyRecordedTime(this.previousFrame, this.playbackTime);
                float num = this.playbackTime;
                if (this.isPlaying) {
                    this.playbackTime += Time.unscaledDeltaTime * this.playBackTimeScale;
                }
                this.playbackTime += Time.unscaledDeltaTime * this.timeScaleAddend;
            }
        }

        // Token: 0x060016AB RID: 5803 RVA: 0x00070A74 File Offset: 0x0006EC74
        private void CheckInput() {
            if (!this.isEditorActive && (PlayerController.Instance.inputController.player.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return))) {
                this.StartReplayEditor();
                return;
            }
            if (this.isEditorActive) {
                if (PlayerController.Instance.inputController.player.GetButtonDown("Right Stick Button")) {
                    this.guiHidden = !this.guiHidden;
                }
                if (PlayerController.Instance.inputController.player.GetButtonDown("B") || Input.GetKeyDown(KeyCode.Escape)) {
                    this.ExitReplayEditor();
                    return;
                }
                if (PlayerController.Instance.inputController.player.GetButtonDown("A")) {
                    if (this.playbackTime == this.clipEndTime) {
                        this.playbackTime = this.clipStartTime;
                    }
                    this.isPlaying = !this.isPlaying;
                }
                if (PlayerController.Instance.inputController.player.GetButton("LB")) {
                    this.isPlaying = false;
                    this.cameraController.enabled = false;
                    float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
                    float axis2 = PlayerController.Instance.inputController.player.GetAxis("RightStickX");
                    if (Mathf.Abs(axis) > 0.01f) {
                        this.clipStartTime = Mathf.Clamp(this.clipStartTime + axis * Time.unscaledDeltaTime, this.recorder.startTime, this.clipEndTime);
                    }
                    if (Mathf.Abs(axis2) > 0.01f) {
                        this.clipEndTime = Mathf.Clamp(this.clipEndTime + axis2 * Time.unscaledDeltaTime, this.clipStartTime, this.recorder.endTime);
                    }
                } else {
                    this.cameraController.enabled = true;
                }
                float f = PlayerController.Instance.inputController.player.GetAxis("RT") - PlayerController.Instance.inputController.player.GetAxis("LT");
                if ((double)Mathf.Abs(f) > 0.001) {
                    this.timeScaleAddend = f;
                } else {
                    this.timeScaleAddend = 0f;
                }
                float axis3 = PlayerController.Instance.inputController.player.GetAxis("DPadX");
                if (Mathf.Abs(axis3) > 0.1f) {
                    if (this.dpadCentered) {
                        ReplayCameraController.KeyStone keyStone = this.cameraController.FindNextKeyStone(this.playbackTime, axis3 < 0f);
                        if (keyStone != null && Mathf.Abs(keyStone.time - this.playbackTime) < this.PlaybackTimeJumpDelta) {
                            this.SetPlaybackTime(keyStone.time);
                        } else {
                            this.SetPlaybackTime(this.playbackTime + this.PlaybackTimeJumpDelta * Mathf.Sign(axis3));
                        }
                    }
                    this.dpadCentered = false;
                    return;
                }
                this.dpadCentered = true;
                if (PlayerController.Instance.inputController.player.GetButtonDown("Start") || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S))) {
                    this.saver.StartSaving();
                }
            }
        }

        // Token: 0x060016AC RID: 5804 RVA: 0x00070D7C File Offset: 0x0006EF7C
        public void OnGUI() {
            if (this.isEditorActive) {
                Cursor.visible = !this.guiHidden;
                if (GUI.Button(new Rect((float)Screen.width / 2f - 10f, (float)Screen.height - 20f, 20f, 20f), this.guiHidden ? "▲" : "▼")) {
                    this.guiHidden = !this.guiHidden;
                }
                if (!this.guiHidden) {
                    int width = Screen.width;
                    float num = (float)(Screen.height - 20);
                    float endTime = this.recorder.endTime;
                    float startTime = this.recorder.startTime;
                    num -= ReplaySkin.DefaultSkin.markerSize.y;
                    num -= 25f;
                    this.GUIClipSliders();
                    Rect position = new Rect(20f, num - 30f, 200f, 20f);
                    GUI.Box(position, "");
                    GUI.Label(position, string.Concat(new object[]
                    {
                    this.playbackTime.ToString("0.#"),
                    "s / ",
                    this.recorder.endTime.ToString("0.#"),
                    "s     ",
                    this.playBackTimeScale.ToString("0.#"),
                    "s/s"
                    }), this.fontMed);
                    float num2 = 40f;
                    float num3 = this.playBackTimeScale;
                    num -= num2 + 10f;
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 7f * num2 / 2f, num, num2, num2), "◀◀")) {
                        this.isPlaying = true;
                        this.playBackTimeScale = -2f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 5f * num2 / 2f, num, num2, num2), "◀")) {
                        this.playBackTimeScale = -1f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 3f * num2 / 2f, num, num2, num2), "◀▮")) {
                        this.playBackTimeScale = -0.5f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 1f * num2 / 2f, num, num2, num2), "▮▮")) {
                        this.playBackTimeScale = 0f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 1f * num2 / 2f, num, num2, num2), "▮▶")) {
                        this.playBackTimeScale = 0.5f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 3f * num2 / 2f, num, num2, num2), "▶")) {
                        this.playBackTimeScale = 1f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 5f * num2 / 2f, num, num2, num2), "▶▶")) {
                        this.playBackTimeScale = 2f;
                    }
                }
            }
        }

        // Token: 0x060016AD RID: 5805 RVA: 0x0007108C File Offset: 0x0006F28C
        public void SetPlaybackTime(float t) {
            this.playbackTime = Mathf.Clamp(t, this.clipStartTime, this.clipEndTime);
            this.previousFrame = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrame);
            this.recorder.ApplyRecordedTime(this.previousFrame, this.playbackTime);
        }

        // Token: 0x060016AE RID: 5806 RVA: 0x000710E8 File Offset: 0x0006F2E8
        public void StartReplayEditor() {
            Cursor.visible = true;
            this.recorder.StopRecording();
            this.isEditorActive = true;
            this.playBackTimeScale = 1f;
            Time.timeScale = 0f;
            this.previousFrame = this.recorder.frameCount - 1;
            this.playbackTime = this.recorder.endTime;
            this.clipStartTime = this.recorder.startTime;
            this.clipEndTime = this.recorder.endTime;
            PlayerController.Instance.animationController.skaterAnim.enabled = false;
            PlayerController.Instance.cameraController.enabled = false;
            InputController.Instance.enabled = false;
            this.cameraController.OnStartReplayEditor();
            SoundManager.Instance.deckSounds.MuteAll();
        }

        // Token: 0x060016AF RID: 5807 RVA: 0x000711B4 File Offset: 0x0006F3B4
        public void ExitReplayEditor() {
            Cursor.visible = false;
            if (this.isEditorActive) {
                this.isEditorActive = false;
                Time.timeScale = 1f;
                this.recorder.isRecording = true;
                PlayerController.Instance.animationController.skaterAnim.enabled = true;
                PlayerController.Instance.cameraController.enabled = true;
                InputController.Instance.enabled = true;
                this.cameraController.OnExitReplayEditor();
                this.recorder.ApplyRecordedFrame(this.recorder.frameCount - 1);
                SoundManager.Instance.deckSounds.UnMuteAll();
            }
        }

        // Token: 0x060016B0 RID: 5808 RVA: 0x00071250 File Offset: 0x0006F450
        private void GUIClipSliders() {
            this.clipStartTime = Mathf.Clamp(GUI.HorizontalSlider(ReplaySkin.DefaultSkin.sliderRect, this.clipStartTime, this.recorder.startTime, this.recorder.endTime, ReplaySkin.DefaultSkin.clipStartSliderStyle, ReplaySkin.DefaultSkin.sliderClipBorderThumbStyle), this.recorder.startTime, this.clipEndTime);
            this.clipEndTime = Mathf.Clamp(GUI.HorizontalSlider(ReplaySkin.DefaultSkin.sliderRect, this.clipEndTime, this.recorder.startTime, this.recorder.endTime, ReplaySkin.DefaultSkin.clipEndSliderStyle, ReplaySkin.DefaultSkin.sliderClipBorderThumbStyle), this.clipStartTime, this.recorder.endTime);
            float num = GUI.HorizontalSlider(ReplaySkin.DefaultSkin.clipSliderRect, this.playbackTime, this.clipStartTime, this.clipEndTime, ReplaySkin.DefaultSkin.clipSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle);
            if (Mathf.Abs(num - this.playbackTime) > 1E-05f) {
                this.SetPlaybackTime(num);
            }
            foreach (ReplayCameraController.KeyStone keyStone in this.cameraController.keyStones) {
                float t = (keyStone.time - this.recorder.startTime) / (this.recorder.endTime - this.recorder.startTime);
                Color textColor = (keyStone is ReplayCameraController.FreeCameraKeyStone) ? Color.blue : ((keyStone is ReplayCameraController.OrbitCameraKeyStone) ? Color.red : Color.green);
                ReplaySkin.DefaultSkin.markerStyle.normal.textColor = textColor;
                if (GUI.Button(ReplaySkin.DefaultSkin.markerRect(t), ReplaySkin.DefaultSkin.markerContent, ReplaySkin.DefaultSkin.markerStyle)) {
                    this.playbackTime = keyStone.time;
                }
            }
        }

        // Token: 0x1700059E RID: 1438
        // (get) Token: 0x060016B1 RID: 5809 RVA: 0x00011506 File Offset: 0x0000F706
        public static ReplayManager Instance {
            get {
                return ReplayManager._instance;
            }
        }

        // Token: 0x040010BB RID: 4283
        private bool showKeys;

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
        public ReplayCameraController cameraController;

        // Token: 0x040010C8 RID: 4296
        private bool isPlaying;

        // Token: 0x040010C9 RID: 4297
        public float clipStartTime;

        // Token: 0x040010CA RID: 4298
        public float clipEndTime;

        // Token: 0x040010CB RID: 4299
        private float timeScaleAddend;

        // Token: 0x040010CC RID: 4300
        private bool dpadCentered = true;

        // Token: 0x040010CD RID: 4301
        public float PlaybackTimeJumpDelta;

        // Token: 0x040010CE RID: 4302
        public ReplaySaver saver;
    }

}
