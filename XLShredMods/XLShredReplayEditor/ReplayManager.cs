﻿using System;
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
        public bool showControllsHelp;
        bool clipEditMode;

        public ReplayCameraController cameraController;

        private bool isPlaying;

        public float clipStartTime;

        public float clipEndTime;

        private float timeScaleAddend;

        /// <summary>
        /// Time = |lastDpadTick|
        /// lastDirection = sign(lastDpadTick)
        /// </summary>
        private float lastDpadTick;

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
                if (PlayerController.Instance.inputController.player.GetButtonDown("Right Stick Button") || Input.GetKeyDown(KeyCode.Return)) {
                    this.guiHidden = !this.guiHidden;
                }
                if (PlayerController.Instance.inputController.player.GetButtonDown("B") || Input.GetKeyDown(KeyCode.Escape)) {
                    this.ExitReplayEditor();
                    return;
                }
                if (PlayerController.Instance.inputController.player.GetButtonDown("A") || Input.GetKeyDown(KeyCode.Space)) {
                    if (this.playbackTime == this.clipEndTime) {
                        this.playbackTime = this.clipStartTime;
                    }
                    this.isPlaying = !this.isPlaying;
                }
                if (PlayerController.Instance.inputController.player.GetButton("LB")) {
                    this.clipEditMode = true;
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
                float dpadX = PlayerController.Instance.inputController.player.GetAxis("DPadX");

                if (Mathf.Abs(dpadX) > 0.3f) {
                    if (Time.unscaledTime - Mathf.Abs(lastDpadTick) > Main.settings.DpadTickRate || Mathf.Sign(dpadX) != Mathf.Sign(lastDpadTick)) {
                        //KeyStone keyStone = this.cameraController.FindNextKeyStone(this.playbackTime, dpadX < 0f);
                        this.lastDpadTick = Time.unscaledTime * Mathf.Sign(dpadX);
                        JumpByTime(Mathf.Sign(dpadX) * Main.settings.PlaybackTimeJumpDelta, true);
                    }
                } else {
                    lastDpadTick = 0f;
                }
                if (Input.GetKeyDown(KeyCode.RightArrow)) {
                    JumpByTime(Main.settings.PlaybackTimeJumpDelta, true);
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                    JumpByTime(-Main.settings.PlaybackTimeJumpDelta, true);
                }
                //if (PlayerController.Instance.inputController.player.GetButtonDown("Start") || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S))) {
                //    this.saver.StartSaving();
                //}
            }
        }

        public void JumpByTime(float deltaTime, bool stopAtMarker) {
            if (stopAtMarker) {
                KeyStone keyStone = this.cameraController.SearchKeyStoneInRange(playbackTime, playbackTime + deltaTime);
                if (keyStone != null) {
                    this.SetPlaybackTime(keyStone.time);
                    return;
                }
            }
            this.SetPlaybackTime(this.playbackTime + deltaTime);
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
                    GUI.Box(ReplaySkin.DefaultSkin.logoRect, "", ReplaySkin.DefaultSkin.kiwiLogoStyle);
                }
                return;
            }
            if (showControllsHelp) {
                DrawControllsGUI();
            }

            DrawButtons();

            if (clipEditMode) {
                DrawClipEditSliders();
            }
            DrawTimeLineSliders();
        }
        void DrawButtons() {
            int width = Screen.width;
            GUILayout.BeginArea(ReplaySkin.DefaultSkin.playPauseRect);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(isPlaying ? "▮▮" : "▶")) {
                isPlaying = !isPlaying;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(ReplaySkin.DefaultSkin.toolsRect);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (clipEditMode) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Cut Clip")) {
                    CutClip();
                    clipEditMode = false;
                }
                if (GUILayout.Button("Cancel Edit")) {
                    clipEditMode = false;
                    clipStartTime = recorder.startTime;
                    clipEndTime = recorder.endTime;
                }
                GUILayout.EndHorizontal();
            } else {
                if (GUILayout.Button("Edit Clip Length (" + (clipEditMode ? "ON" : "OFF") + ")")) {
                    clipEditMode = true;
                }
            }

            if (GUILayout.Button("Show Help (" + (showControllsHelp ? "ON" : "OFF") + ")")) {
                showControllsHelp = !showControllsHelp;
            }

            //showControllsHelp Toggle
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(ReplaySkin.DefaultSkin.timeScaleRect);
            GUILayout.BeginHorizontal();

            DrawTimeScaleGUI();

            GUILayout.FlexibleSpace();
            //showControllsHelp Toggle
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

        }
        void DrawTimeScaleGUI() {
            GUILayout.Label(String.Format("{0:0.#}s /{1:0.#}s | Speed: {2:0.#}s/s", displayedPlaybackTime, recorder.recordedTime, playbackSpeed), ReplaySkin.DefaultSkin.fontMed, GUILayout.Width(200));
            //playbackSpeed
            //if (float.TryParse(GUILayout.TextField(playbackSpeed.ToString("0.0"), GUILayout.Width(40)), out float newValue)) {
            //    playbackSpeed = newValue;
            //}
            float value = GUILayout.HorizontalSlider(playbackSpeed, 0f, 2f, ReplaySkin.DefaultSkin.timeScaleSliderStyle, GUI.skin.horizontalSliderThumb, GUILayout.MinWidth(300), GUILayout.MaxWidth(500));
            playbackSpeed = Mathf.Round(value * 10f) / 10f;
        }
        void DrawControllsGUI() {
            var style = new GUIStyle(GUI.skin.window);
            style.normal.background = GUI.skin.box.normal.background;
            style.focused.background = GUI.skin.box.normal.background;
            GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), ReplaySkin.DefaultSkin.controllsRect, ReplayControllsWindow, "Controlls:", style);
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
            GUI.Box(clipStartRect, "", ReplaySkin.DefaultSkin.clipCutStyle);
            GUI.Box(clipEndRect, "", ReplaySkin.DefaultSkin.clipCutStyle);

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
            GUILayout.Space(20);
            DrawControllGUI("ControllName", "Keyboard", "Xbox", "PS4");
            GUILayout.Space(10);
            DrawControllGUI("Use KeyFrame Animation (" + (cameraController.CamFollowKeyStones ? "On" : "Off") + ")", "", "select", "share");
            DrawControllGUI("Show/Hide GUI", "Return", "RS", "R3");
            DrawControllGUI("Change Mode", "M", "Y", "\u25B3");
            DrawControllGUI("Add KeyStone", "K", "X", "\u25A1");
            DrawControllGUI("Delete KeyStone", "Delete", "Hold X", "Hold \u25A1");
            DrawControllGUI(String.Format("DPadX: Jump to next KeyStone or max {0:0.#} s", Main.settings.PlaybackTimeJumpDelta), "Arrows", "DPadX", "DPadX");
            DrawControllGUI("Change Start of clip", "", "LB + LeftStickX", "LB + LeftStickX");
            DrawControllGUI("Change End of clip", "", "LB + RightStickX", "LB + RightStickX");

            switch (cameraController.mode) {
                case ReplayCameraController.CameraMode.Free:
                    DrawControllGUI("Move(xz)", "", "LeftStick", "LeftStick");
                    DrawControllGUI("Move(y)", "", "DpadY", "DpadY");
                    DrawControllGUI("Rotate", "", "RightStick", "RightStick");
                    DrawControllGUI("Roll", "", "RB + RightStickX", "RB + RightStickX");
                    break;
                case ReplayCameraController.CameraMode.Orbit:
                    DrawControllGUI("Orbit around Skater", "", "LeftStickX + RightStickY", "LeftStickX + RightStickY");
                    DrawControllGUI("Change Orbit Radius", "", "LeftStickY", "LeftStickY");
                    DrawControllGUI("Change Focus Offset", "", "RB + RightStickY", "RB + RightStickY");
                    break;
                case ReplayCameraController.CameraMode.Tripod:
                    DrawControllGUI("Move(xz)", "", "LeftStick", "LeftStick");
                    DrawControllGUI("Move(y)", "", "DpadY", "DpadY");
                    DrawControllGUI("Change Focus Offset", "", "RB + RightStickY", "RB + RightStickY");
                    break;
            }
            DrawControllGUI("Change camera FOV", "", "RB + LeftStickY", "RB + LeftStickY");
            GUILayout.EndVertical();
        }
        private void DrawControllGUI(string name, string keyControll, string xboxControll, string ps4Controll) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();

            GUIStyle keyStyle = new GUIStyle(GUI.skin.label);
            keyStyle.normal.textColor = Color.red;
            GUIStyle xboxStyle = new GUIStyle(GUI.skin.label);
            xboxStyle.normal.textColor = Color.green;
            GUIStyle ps4Style = new GUIStyle(GUI.skin.label);
            ps4Style.normal.textColor = Color.cyan;

            GUILayout.Label(keyControll, keyStyle);
            GUILayout.Label(xboxControll, xboxStyle);
            GUILayout.Label(ps4Controll, ps4Style);
            GUILayout.EndHorizontal();
        }
        #endregion
    }

}
