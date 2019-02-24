using System;
using System.Collections;
using UnityEngine;
using UnityModManagerNet;
using XLShredLib;
using GUILayoutLib;

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
        bool clipEditMode;

        public ReplayCameraController cameraController;

        private bool isPlaying;

        public float clipStartTime;

        public float clipEndTime;

        private float timeScaleAddend;

        private bool dpadCentered = true;

        public float PlaybackTimeJumpDelta;

        public ReplaySaver saver;


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

            PlayerController.Instance.respawn.pin.gameObject.SetActive(false);

            this.cameraController.OnStartReplayEditor();
            audioRecorder.StopRecording();
            yield return audioRecorder.StartPlayback();

            this.playbackSpeed = 1f;
            Time.timeScale = 0f;
            this.previousFrameIndex = this.recorder.frameCount - 1;
            this.playbackTime = this.recorder.endTime;
            this.clipStartTime = this.recorder.startTime;
            this.clipEndTime = this.recorder.endTime;

            XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", true);
            ModMenu.Instance.RegisterTimeScaleTarget(Main.modId, () => 0f);
            ModMenu.Instance.RegisterShowCursor(Main.modId, () => (CurrentState == ReplayState.PLAYBACK && !guiHidden) ? 1 : 0);
            ReplayManager.SetState(ReplayState.PLAYBACK);
        }


        public void ExitReplayEditor() {
            try {
                ReplayManager.SetState(ReplayState.LOADING);

                PlayerController.Instance.respawn.pin.gameObject.SetActive(true);

                audioRecorder.StopPlayback();
                audioRecorder.StartRecording();
                PlayerController.Instance.animationController.skaterAnim.enabled = true;
                PlayerController.Instance.cameraController.enabled = true;
                InputController.Instance.enabled = true;
                this.cameraController.OnExitReplayEditor();
                this.recorder.ApplyLastFrame();
                SoundManager.Instance.deckSounds.UnMuteAll();
                XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", false);
                ModMenu.Instance.UnregisterTimeScaleTarget(Main.modId);
                Time.timeScale = 1f;
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
                    float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX") * recorder.recordedTime / 3f;
                    float axis2 = PlayerController.Instance.inputController.player.GetAxis("RightStickX") * recorder.recordedTime / 3f;
                    float saveZone = recorder.recordedTime / 50f;
                    if (Mathf.Abs(axis) > 0.01f) {
                        this.clipStartTime = Mathf.Clamp(this.clipStartTime + axis * Time.unscaledDeltaTime, this.recorder.startTime, this.clipEndTime - saveZone);
                    }
                    if (Mathf.Abs(axis2) > 0.01f) {
                        this.clipEndTime = Mathf.Clamp(this.clipEndTime + axis2 * Time.unscaledDeltaTime, this.clipStartTime + saveZone, this.recorder.endTime);
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

        public void SetPlaybackTime(float t) {
            this.playbackTime = Mathf.Clamp(t, this.clipStartTime, this.clipEndTime);
            this.previousFrameIndex = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrameIndex);
            this.recorder.ApplyRecordedTime(this.previousFrameIndex, this.playbackTime);
            this.audioRecorder.SetPlaybackTime(playbackTime);
        }

        public void CutClip() {
            recorder.startTime = clipStartTime;
            recorder.endTime = clipEndTime;
        }
        public void Destroy() {
            Destroy(recorder);
            audioRecorder.Destroy();
            SetState(ReplayState.DISABLED);
            Destroy(gameObject);
        }

        #region GUI
        public void OnGUI() {
            if (CurrentState != ReplayState.PLAYBACK) { return; }
            if (this.guiHidden) {
                if (Main.settings.showLogo) {
                    if (GUI.Button(ReplaySkin.DefaultSkin.logoRect, "", ReplaySkin.DefaultSkin.kiwiLogoStyle)) {
                        ReplaySkin.DefaultSkin.cycleThroughLogoWidth();
                    }
                }
                return;
            }

            DrawTimeLineGUI();
            DrawControllsGUI();
        }
        void DrawTimeLineGUI() {
            int width = Screen.width;
            GUILayout.BeginArea(ReplaySkin.DefaultSkin.playPauseRect);
            GUILayout.BeginVertical();

            if (clipEditMode) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Cut Clip")) {
                    CutClip();
                    clipEditMode = false;
                }
                if (GUILayout.Button("Cancel")) {
                    clipEditMode = false;
                    clipStartTime = recorder.startTime;
                    clipEndTime = recorder.endTime;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(isPlaying ? "▮▮" : "▶")) {
                isPlaying = !isPlaying;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(ReplaySkin.DefaultSkin.toolsRect);
            GUILayout.BeginHorizontal();

            DrawTimeScaleGUI();
            GUILayout.FlexibleSpace();
            DrawClipEditButtonGUI();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (clipEditMode) {
                DrawClipEditSliders();
            }
            DrawTimeLineSliders();
        }
        void DrawTimeScaleGUI() {
            GUILayout.Label(String.Format("{0:0.#}s /{1:0.#}s | TimeScale: {2:0.#}s/s", displayedPlaybackTime, recorder.recordedTime, playbackTimeScale), ReplaySkin.DefaultSkin.fontMed, GUILayout.Width(200));
            //playbackSpeed
            //if (float.TryParse(GUILayout.TextField(playbackSpeed.ToString("0.0"), GUILayout.Width(40)), out float newValue)) {
            //    playbackSpeed = newValue;
            //}
            float value = GUILayout.HorizontalSlider(playbackSpeed, 0f, 2f, ReplaySkin.DefaultSkin.timeScaleSliderStyle, GUI.skin.horizontalSliderThumb, GUILayout.MinWidth(300), GUILayout.MaxWidth(500));
            playbackSpeed = Mathf.Round(value * 10f) / 10f;
        }
        void DrawClipEditButtonGUI() {
            if (GUILayout.Button("Edit Clip Length")) {
                clipEditMode = !clipEditMode;
            }
        }
        void DrawControllsGUI() {
            GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), ReplaySkin.DefaultSkin.controllsRect, ReplayControllsWindow, "Controlls:");
        }
        void DrawClipEditSliders() {
            float saveZone = recorder.recordedTime / 50f;
            var clipRects = ReplaySkin.DefaultSkin.clipSliderRects(recorder.startTime, recorder.endTime, clipStartTime, clipEndTime, saveZone);

            Rect clipStartRect = ReplaySkin.DefaultSkin.MarkerRectForNormT(clipStartTime);
            clipStartRect.yMax = ReplaySkin.DefaultSkin.sliderRect.yMax;
            Rect clipEndRect = ReplaySkin.DefaultSkin.MarkerRectForNormT(clipEndTime);
            clipStartRect.yMax = ReplaySkin.DefaultSkin.sliderRect.yMax;

            Rect clipStartCutRect = new Rect() {
                xMin = ReplaySkin.DefaultSkin.sliderRect.xMin,
                xMax = clipStartRect.center.x,
                yMin = clipStartRect.yMin,
                yMax = clipStartRect.yMax
            };
            Rect clipEndCutRect = new Rect() {
                xMin = ReplaySkin.DefaultSkin.sliderRect.xMin,
                xMax = clipStartRect.center.x,
                yMin = clipStartRect.yMin,
                yMax = clipStartRect.yMax
            };

            this.clipStartTime = Mathf.Clamp(GUI.HorizontalSlider(clipRects[0], this.clipStartTime, this.recorder.startTime, this.clipEndTime - saveZone, ReplaySkin.DefaultSkin.transparentSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle), this.recorder.startTime, this.clipEndTime);

            this.clipEndTime = Mathf.Clamp(GUI.HorizontalSlider(clipRects[1], this.clipEndTime, this.clipStartTime + saveZone, this.recorder.endTime, ReplaySkin.DefaultSkin.transparentSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle), this.clipStartTime, this.recorder.endTime);

            GUI.Box(clipRects[2], "", ReplaySkin.DefaultSkin.clipBoxStyle);
            //GUI.Box(clipStartRect, "", ReplaySkin.DefaultSkin.clipCutStyle);
            //GUI.Box(clipEndRect, "", ReplaySkin.DefaultSkin.clipCutStyle);

            //GUIHelper.DraggableArea(
            //    "clipStart",
            //    clipStartRect,
            //    delegate (out bool beginDrag) {  //GUI Draw
            //        GUILayout.BeginVertical();
            //        beginDrag = GUILayout.RepeatButton("O", ReplaySkin.DefaultSkin.markerStyle);
            //        GUILayout.Label("|", ReplaySkin.DefaultSkin.markerStyle, GUILayout.ExpandHeight(true));
            //        GUILayout.EndVertical();
            //    }, delegate () { //OnClick
            //        SetPlaybackTime(clipStartTime);
            //    },
            //    delegate (ref Rect r) {  //OnDragUpdate
            //        float xmin = ReplaySkin.DefaultSkin.sliderRect.xMin + (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
            //        float xmax = ReplaySkin.DefaultSkin.sliderRect.xMax - (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
            //        r.center = new Vector2(Mathf.Clamp(r.center.x, xmin, xmax), clipStartRect.center.y);
            //        float time = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
            //        SetPlaybackTime(time);
            //    },
            //    delegate (Rect r) {  //OnDrop
            //        clipStartTime = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
            //        //TODO remove keyframe cache
            //    },
            //    this,
            //    true);

            //GUIHelper.DraggableArea(
            //    "clipStart",
            //    clipStartRect,
            //    delegate (out bool beginDrag) {  //GUI Draw
            //        GUILayout.BeginVertical();
            //        beginDrag = GUILayout.RepeatButton("O", ReplaySkin.DefaultSkin.markerStyle);
            //        GUILayout.Label("|", ReplaySkin.DefaultSkin.markerStyle, GUILayout.ExpandHeight(true));
            //        GUILayout.EndVertical();
            //    }, delegate () { //OnClick
            //        SetPlaybackTime(clipEndTime);
            //    },
            //    delegate (ref Rect r) {  //OnDragUpdate
            //        float xmin = ReplaySkin.DefaultSkin.sliderRect.xMin + (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
            //        float xmax = ReplaySkin.DefaultSkin.sliderRect.xMax - (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
            //        r.center = new Vector2(Mathf.Clamp(r.center.x, xmin, xmax), clipStartRect.center.y);
            //        float time = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
            //        SetPlaybackTime(time);
            //    },
            //    delegate (Rect r) {  //OnDrop
            //        clipEndTime = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
            //        //TODO remove keyframe cache
            //    },
            //    this,
            //    true);

        }
        private void DrawTimeLineSliders() {

            float pTime = GUI.HorizontalSlider(ReplaySkin.DefaultSkin.sliderRect, this.playbackTime, this.recorder.startTime, this.recorder.endTime, ReplaySkin.DefaultSkin.clipSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle);
            if (Mathf.Abs(pTime - this.playbackTime) > 1E-05f) {
                this.SetPlaybackTime(pTime);
            }

            foreach (KeyStone keyStone in this.cameraController.keyStones) {
                float t = (keyStone.time - this.recorder.startTime) / (this.recorder.endTime - this.recorder.startTime);
                Color textColor = (keyStone is FreeCameraKeyStone) ? Color.blue : ((keyStone is OrbitCameraKeyStone) ? Color.red : Color.green);
                ReplaySkin.DefaultSkin.markerStyle.normal.textColor = textColor;
                Rect markerRect = ReplaySkin.DefaultSkin.MarkerRectForNormT(t);

                GUIHelper.DraggableArea(
                    "keyframe" + keyStone.GetHashCode(),
                    markerRect,
                    delegate (out bool beginDrag) {  //GUI Draw
                        GUILayout.BeginVertical();
                        GUILayout.Label("|", ReplaySkin.DefaultSkin.markerStyle, GUILayout.ExpandHeight(true));
                        beginDrag = GUILayout.RepeatButton("O", ReplaySkin.DefaultSkin.markerStyle);
                        GUILayout.EndVertical();
                    },
                    () => SetPlaybackTime(keyStone.time),
                    delegate (ref Rect r) {  //OnDragUpdate
                        float xmin = ReplaySkin.DefaultSkin.sliderRect.xMin + (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
                        float xmax = ReplaySkin.DefaultSkin.sliderRect.xMax - (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
                        r.center = new Vector2(Mathf.Clamp(r.center.x, xmin, xmax), markerRect.center.y);
                        float time = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
                        SetPlaybackTime(time);
                    },
                    delegate (Rect r) {  //OnDrop
                        keyStone.time = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
                        //TODO remove keyframe cache
                    },
                    this,
                    true);

                //if (GUI.Button(ReplaySkin.DefaultSkin.MarkerRectForNormT(t), ReplaySkin.DefaultSkin.markerContent, ReplaySkin.DefaultSkin.markerStyle)) {
                //    this.playbackTime = keyStone.time;
                //}
            }
        }

        public void ReplayControllsWindow(int id) {
            GUILayout.BeginVertical();
            float y = 20f;
            GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label("CamMode: " + Enum.GetName(typeof(ReplayCameraController.CameraMode), cameraController.mode));
            GUILayout.Label("Back: Enable KeyStone Animation (" + (cameraController.CamFollowKeyStones ? "On" : "Off") + ")");
            GUILayout.Label("RightStick: Show/Hide GUI");
            GUILayout.Label("Y: Change Mode");
            GUILayout.Label("X: Add KeyStone");
            GUILayout.Label("Hold X: Delete KeyStone");
            GUILayout.Label("DPadX: Jump to next KeyStone or max 5s");
            GUILayout.Label("LB + LeftStickX Change Start of clip");
            GUILayout.Label("LB + RightStickX Change End of clip");
            GUILayout.Space(20);
            switch (cameraController.mode) {
                case ReplayCameraController.CameraMode.Free:
                    GUILayout.Label("LeftStick: Move(xz)");
                    GUILayout.Label("DpadY: Move(y)");
                    GUILayout.Label("RightStick: Rotate");
                    GUILayout.Label("RB + RightStickX Rotate around forward axis");
                    break;
                case ReplayCameraController.CameraMode.Orbit:
                    GUILayout.Label("LeftStickX + RightStickY: Orbit around Skater");
                    GUILayout.Label("LeftStickY: Change Orbit Radius");
                    GUILayout.Label("RB + RightStickY Change Focus Offset");
                    break;
                case ReplayCameraController.CameraMode.Tripod:
                    GUILayout.Label("LeftStick: Move(xz)");
                    GUILayout.Label("DpadY or RightStickY: Move(y)");
                    GUILayout.Label("RB + RightStickY Change Focus Offset");
                    break;
            }
            GUILayout.Label("RB + LeftStickY Change camera FOV");
            GUILayout.EndVertical();
        }
        #endregion
    }

}
