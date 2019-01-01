using System;
using UnityEngine;

namespace XLShredReplayEditor {

    public class ReplayManager : MonoBehaviour {

        public ReplayManager() {
            this.showKeys = true;
        }


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


        public void OnGUI() {
            if (this.isEditorActive) {
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


        public void SetPlaybackTime(float t) {
            this.playbackTime = Mathf.Clamp(t, this.clipStartTime, this.clipEndTime);
            this.previousFrame = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrame);
            this.recorder.ApplyRecordedTime(this.previousFrame, this.playbackTime);
        }


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



        public static ReplayManager Instance {
            get {
                return ReplayManager._instance;
            }
        }


        private bool showKeys;


        private GUIStyle fontLarge;


        private GUIStyle fontMed;


        private GUIStyle fontSmall;


        private Color guiColor;


        private static ReplayManager _instance;


        public ReplayRecorder recorder;


        public float playbackTime;


        public float playBackTimeScale;


        public int previousFrame;


        private bool isEditorActive;


        public bool guiHidden;


        public ReplayCameraController cameraController;


        private bool isPlaying;


        public float clipStartTime;


        public float clipEndTime;


        private float timeScaleAddend;


        private bool dpadCentered = true;


        public float PlaybackTimeJumpDelta;


        public ReplaySaver saver;
    }

}
