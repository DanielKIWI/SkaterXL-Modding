using System;
using System.Collections;
using UnityEngine;
using UnityModManagerNet;
#if !STANDALONE
using XLShredLib;
#endif

namespace XLShredReplayEditor {

    public enum ReplayState {
        LOADING, RECORDING, PLAYBACK, DISABLED
    }

    public class ReplayManager : MonoBehaviour {
        private static ReplayState _currentState = ReplayState.DISABLED;
        public static ReplayState CurrentState { get { return _currentState; } }
        public static void SetState(ReplayState s) {
            if (_currentState == s) return;
            Debug.Log("Changed ReplayState to " + s.ToString());
            ReplayState oldState = _currentState;
            _currentState = s;
            StateChangedEvent?.Invoke(s, oldState);
        }
        public delegate void ReplayStateChangedEventHandler(ReplayState newState, ReplayState oldState);
        public static event ReplayStateChangedEventHandler StateChangedEvent;

        public static ReplayManager Instance {
            get {
                return ReplayManager._instance;
            }
        }
        private static ReplayManager _instance;

        private GUIStyle fontLarge;
        private GUIStyle fontMed;
        private GUIStyle fontSmall;
        private Color guiColor;


        public ReplayRecorder recorder;
        public ReplayAudioRecorder audioRecorder;

        public float playbackTime;
        public float displayedPlaybackTime {
            get {
                return playbackTime - recorder.startTime;
            }
        }
        private float playbackSpeed;
        public float playbackTimeScale {
            get {
                float timeScale = this.timeScaleAddend;
                if (this.isPlaying) {
                    timeScale += this.playbackSpeed;
                }
                return timeScale;
            }
        }

        public int previousFrameIndex;
        public ReplayRecordedFrame CurrentFrame {
            get {
                return recorder.recordedFrames[previousFrameIndex];
            }
        }

        public bool guiHidden;

        public ReplayCameraController cameraController;

        private bool isPlaying;

        public float clipStartTime;

        public float clipEndTime;

        private float timeScaleAddend;

        private bool dpadCentered = true;

        public float PlaybackTimeJumpDelta;

        public ReplaySaver saver;

        public ReplayManager() {
        }

        public void Awake() {
            DontDestroyOnLoad(gameObject);
            ReplayManager._instance = this;
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
            if (audioRecorder == null) {
                audioRecorder = PlayerController.Instance.skaterController.skaterTransform.gameObject.AddComponent<ReplayAudioRecorder>();
                audioRecorder.enabled = true;
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
            this.PlaybackTimeJumpDelta = 5f;
        }
        public void Start() {
            if (Main.enabled) {
                ReplayManager.SetState(ReplayState.RECORDING);
                audioRecorder.StartRecording();
            }
            XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", false);
        }

        public IEnumerator StartReplayEditor() {
            ReplayManager.SetState(ReplayState.LOADING);
            Debug.Log("Started Replay Editor");

            //Disabling core Game Input and animation that would interfer
            PlayerController.Instance.animationController.skaterAnim.enabled = false;
            PlayerController.Instance.cameraController.enabled = false;
            InputController.Instance.enabled = false;
            SoundManager.Instance.deckSounds.MuteAll();

            Cursor.visible = true;

            this.cameraController.OnStartReplayEditor();
            audioRecorder.StopRecording();
            yield return audioRecorder.StartPlayback();

            this.playbackSpeed = 1f;
            Time.timeScale = 0f;
            this.previousFrameIndex = this.recorder.frameCount - 1;
            this.playbackTime = this.recorder.endTime;
            this.clipStartTime = this.recorder.startTime;
            this.clipEndTime = this.recorder.endTime;

#if STANDALONE
            Time.timeScale = 0f;
#else
            XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", true);
            ModMenu.Instance.RegisterTimeScaleTarget(Main.modId, () => 0f);
            ModMenu.Instance.RegisterShowCursor(Main.modId, () => (CurrentState == ReplayState.PLAYBACK && !guiHidden) ? 1 : 0);
#endif
            ReplayManager.SetState(ReplayState.PLAYBACK);
        }


        public void ExitReplayEditor() {
            try {
                ReplayManager.SetState(ReplayState.LOADING);
                audioRecorder.StopPlayback();
                audioRecorder.StartRecording();
                PlayerController.Instance.animationController.skaterAnim.enabled = true;
                PlayerController.Instance.cameraController.enabled = true;
                InputController.Instance.enabled = true;
                this.cameraController.OnExitReplayEditor();
                this.recorder.ApplyLastFrame();
                SoundManager.Instance.deckSounds.UnMuteAll();
#if STANDALONE
            Cursor.visible = false;
            Time.timeScale = 1f;
#else
                XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", false);
                ModMenu.Instance.UnregisterTimeScaleTarget(Main.modId);
                Time.timeScale = 1f;
#endif
                ReplayManager.SetState(ReplayState.RECORDING);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public void Update() {
            if (!Main.enabled) {
                return;
            }
            this.CheckInput();
            this.playbackTime += playbackTimeScale * Time.unscaledDeltaTime;
            if (CurrentState == ReplayState.PLAYBACK) {
                if (this.playbackTime < this.clipStartTime && this.playbackTimeScale < 0f) {
                    this.isPlaying = false;
                    this.playbackTime = this.clipStartTime;
                } else if (this.playbackTime > this.clipEndTime && this.playbackTimeScale > 0f) {
                    this.isPlaying = false;
                    this.playbackTime = this.clipEndTime;
                }
                //Settin SkaterTransforms
                this.previousFrameIndex = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrameIndex);
                this.recorder.ApplyRecordedTime(this.previousFrameIndex, this.playbackTime);
            }
        }


        private void CheckInput() {
            if (CurrentState != ReplayState.PLAYBACK && (PlayerController.Instance.inputController.player.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return))) {
                StartCoroutine(StartReplayEditor());
                return;
            }
            if (CurrentState == ReplayState.PLAYBACK) {
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
                        KeyStone keyStone = this.cameraController.FindNextKeyStone(this.playbackTime, axis3 < 0f);
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
                //if (PlayerController.Instance.inputController.player.GetButtonDown("Start") || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S))) {
                //    this.saver.StartSaving();
                //}
            }
        }


        public void OnGUI() {
            if (CurrentState == ReplayState.PLAYBACK) {
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
                    (displayedPlaybackTime).ToString("0.#"),
                    "s / ",
                    (this.recorder.recordedTime).ToString("0.#"),
                    "s     ",
                    this.playbackTimeScale.ToString("0.#"),
                    "s/s"
                    }), this.fontMed);
                    float num2 = 40f;
                    float num3 = this.playbackSpeed;
                    num -= num2 + 10f;
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 7f * num2 / 2f, num, num2, num2), "◀◀")) {
                        this.isPlaying = true;
                        this.playbackSpeed = -2f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 5f * num2 / 2f, num, num2, num2), "◀")) {
                        this.playbackSpeed = -1f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 3f * num2 / 2f, num, num2, num2), "◀▮")) {
                        this.playbackSpeed = -0.5f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f - 1f * num2 / 2f, num, num2, num2), "▮▮")) {
                        this.playbackSpeed = 0f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 1f * num2 / 2f, num, num2, num2), "▮▶")) {
                        this.playbackSpeed = 0.5f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 3f * num2 / 2f, num, num2, num2), "▶")) {
                        this.playbackSpeed = 1f;
                    }
                    if (GUI.Button(new Rect((float)Screen.width / 2f + 5f * num2 / 2f, num, num2, num2), "▶▶")) {
                        this.playbackSpeed = 2f;
                    }
                }
            }
        }

        public void SetPlaybackTime(float t) {
            this.playbackTime = Mathf.Clamp(t, this.clipStartTime, this.clipEndTime);
            this.previousFrameIndex = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrameIndex);
            this.recorder.ApplyRecordedTime(this.previousFrameIndex, this.playbackTime);
            this.audioRecorder.SetPlaybackTime(t);
        }

        private void GUIClipSliders() {
            this.clipStartTime = Mathf.Clamp(GUI.HorizontalSlider(ReplaySkin.DefaultSkin.sliderRect, this.clipStartTime, this.recorder.startTime, this.recorder.endTime, ReplaySkin.DefaultSkin.clipStartSliderStyle, ReplaySkin.DefaultSkin.sliderClipBorderThumbStyle), this.recorder.startTime, this.clipEndTime);
            this.clipEndTime = Mathf.Clamp(GUI.HorizontalSlider(ReplaySkin.DefaultSkin.sliderRect, this.clipEndTime, this.recorder.startTime, this.recorder.endTime, ReplaySkin.DefaultSkin.clipEndSliderStyle, ReplaySkin.DefaultSkin.sliderClipBorderThumbStyle), this.clipStartTime, this.recorder.endTime);
            float num = GUI.HorizontalSlider(ReplaySkin.DefaultSkin.clipSliderRect, this.playbackTime, this.clipStartTime, this.clipEndTime, ReplaySkin.DefaultSkin.clipSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle);
            if (Mathf.Abs(num - this.playbackTime) > 1E-05f) {
                this.SetPlaybackTime(num);
            }
            foreach (KeyStone keyStone in this.cameraController.keyStones) {
                float t = (keyStone.time - this.recorder.startTime) / (this.recorder.endTime - this.recorder.startTime);
                Color textColor = (keyStone is FreeCameraKeyStone) ? Color.blue : ((keyStone is OrbitCameraKeyStone) ? Color.red : Color.green);
                ReplaySkin.DefaultSkin.markerStyle.normal.textColor = textColor;
                if (GUI.Button(ReplaySkin.DefaultSkin.markerRect(t), ReplaySkin.DefaultSkin.markerContent, ReplaySkin.DefaultSkin.markerStyle)) {
                    this.playbackTime = keyStone.time;
                }
            }
        }

        public void Destroy() {
            Destroy(recorder);
            audioRecorder.Destroy();
            SetState(ReplayState.DISABLED);
        }
    }

}
