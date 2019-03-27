using System;
using System.Collections;
using UnityEngine;
using UnityModManagerNet;
using XLShredLib;
using GUILayoutLib;
using System.Collections.Generic;
using System.Linq;

namespace XLShredReplayEditor {
    using Utils;

    public enum ReplayState {
        LOADING, RECORDING, PLAYBACK, DISABLED
    }

    public class ReplayManager : MonoBehaviour {
        private static ReplayState _currentState = ReplayState.DISABLED;
        public static ReplayState CurrentState {
            get { return _currentState; }
            private set {
                if (_currentState == value) return;
                Debug.Log("Changed ReplayState to " + value.ToString());
                ReplayState oldState = _currentState;
                _currentState = value;
                StateChangedEvent?.Invoke(value, oldState);
            }
        }
        public delegate void ReplayStateChangedEventHandler(ReplayState newState, ReplayState oldState);
        public static event ReplayStateChangedEventHandler StateChangedEvent;

        public static ReplayManager Instance { get; private set; }

        public ReplayRecorder recorder;
        public ReplayAudioRecorder audioRecorder;
        public ReplayCameraController cameraController;
        public ReplayEditorMenu menu;

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

        public bool guiHidden;
        bool clipEditMode;
        private bool isPlaying;

        public float clipStartTime;
        public float clipEndTime;

        private float timeScaleAddend;

        /// <summary>
        /// Time = |lastDpadTick|, 
        /// lastDirection = sign(lastDpadTick)
        /// </summary>
        private float lastDpadTick;
        
        public void Awake() {
            DontDestroyOnLoad(gameObject);
            ReplayManager.Instance = this;
            this.recorder = gameObject.AddComponent<ReplayRecorder>();
            this.cameraController = gameObject.AddComponent<ReplayCameraController>();
            this.cameraController.enabled = false;
            this.menu = gameObject.AddComponent<ReplayEditorMenu>();
            this.menu.enabled = false;
            audioRecorder = PlayerController.Instance.skaterController.skaterTransform.gameObject.AddComponent<ReplayAudioRecorder>();
            audioRecorder.enabled = true;
            ReplayManager.CurrentState = ReplayState.RECORDING;
            audioRecorder.StartRecording();
        }

        public void Destroy() {
            recorder.Destroy();
            audioRecorder.Destroy();
            ReplayManager.CurrentState = ReplayState.DISABLED;
            Destroy(gameObject);
        }

        public IEnumerator StartReplayEditor() {
            ReplayManager.CurrentState = ReplayState.LOADING;
            Debug.Log("Started Replay Editor");
            Cursor.visible = true;

            //Disabling core Game Input and animation that would interfer
            SoundManager.Instance.deckSounds.MuteAll();
            PlayerController.Instance.cameraController.enabled = false;
            InputController.Instance.enabled = false;
            PlayerController.Instance.enabled = false;
            PlayerController.Instance.respawn.pin.gameObject.SetActive(false);

            recorder.OnStartReplayEditor();
            cameraController.OnStartReplayEditor();
            audioRecorder.StopRecording();
            yield return audioRecorder.StartPlayback();

            this.playbackSpeed = 1f;
            Time.timeScale = 0f;
            this.previousFrameIndex = this.recorder.ClipFrames.Count - 1;
            this.playbackTime = this.recorder.endTime;
            this.clipStartTime = this.recorder.startTime;
            this.clipEndTime = this.recorder.endTime;

            ModMenu.Instance.RegisterTimeScaleTarget(Main.modId, () => 0f);
            ModMenu.Instance.RegisterShowCursor(Main.modId, () => (CurrentState == ReplayState.PLAYBACK && !guiHidden) ? 1 : 0);
            ReplayManager.CurrentState = ReplayState.PLAYBACK;
            XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", true);
        }
        
        public void ExitReplayEditor() {
            try {
                ReplayManager.CurrentState = ReplayState.LOADING;
                
                audioRecorder.StopPlayback();
                audioRecorder.StartRecording();
                cameraController.OnExitReplayEditor();
                recorder.OnExitReplayEditor();

                PlayerController.Instance.respawn.pin.gameObject.SetActive(true);
                PlayerController.Instance.cameraController.enabled = true;
                InputController.Instance.enabled = true;
                PlayerController.Instance.enabled = true;
                SoundManager.Instance.deckSounds.UnMuteAll();

                ModMenu.Instance.UnregisterTimeScaleTarget(Main.modId);
                Time.timeScale = 1f;

                ReplayManager.CurrentState = ReplayState.RECORDING;
                XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", false);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public void Update() {
            this.CheckInput();
            if (CurrentState != ReplayState.PLAYBACK) return;

            this.playbackTime += playbackTimeScale * Time.unscaledDeltaTime;
            if (this.playbackTime < this.clipStartTime && this.playbackTimeScale < 0f) {
                this.isPlaying = false;
                this.playbackTime = this.clipStartTime;
            } else if (this.playbackTime > this.clipEndTime && this.playbackTimeScale > 0f) {
                this.isPlaying = false;
                this.playbackTime = this.clipEndTime;
            }
            this.previousFrameIndex = this.recorder.GetFrameIndex(this.playbackTime, this.previousFrameIndex);

            this.recorder.ApplyRecordedTime(this.previousFrameIndex, this.playbackTime);
        }

        //public void LateUpdate() {
        //    if (CurrentState != ReplayState.PLAYBACK) return;

        //    //Setting SkaterTransforms
        //    this.recorder.ApplyRecordedTime(this.previousFrameIndex, this.playbackTime);
        //}

        private void CheckInput() {
            if (CurrentState == ReplayState.RECORDING && (PlayerController.Instance.inputController.player.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return))) {
                StartCoroutine(StartReplayEditor());
                return;
            }
            if (CurrentState == ReplayState.PLAYBACK) {
                if (PlayerController.Instance.inputController.player.GetButtonDown("B") || Input.GetKeyDown(KeyCode.Escape)) {
                    this.ExitReplayEditor();
                    return;
                }

                if (CurrentState == ReplayState.PLAYBACK && PlayerController.Instance.inputController.player.GetButtonDown("Start")) {
                    menu.Toggle();
                }

                if (PlayerController.Instance.inputController.player.GetButtonDown("Right Stick Button") || Input.GetKeyDown(KeyCode.Return)) {
                    this.guiHidden = !this.guiHidden;
                }
                if (PlayerController.Instance.inputController.player.GetButtonDown("A") || Input.GetKeyDown(KeyCode.Space)) {
                    if (this.playbackTime == this.clipEndTime) {
                        this.playbackTime = this.clipStartTime;
                    }
                    this.isPlaying = !this.isPlaying;
                }
                
                //Save
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.S)) {
                    menu.OpenSaveMenu();
                }

                //ClipEdit
                if (clipEditMode && PlayerController.Instance.inputController.player.GetButton("LB")) {
                    this.isPlaying = false;
                    float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX") * recorder.recordedTime / 3f;
                    float axis2 = PlayerController.Instance.inputController.player.GetAxis("RightStickX") * recorder.recordedTime / 3f;
                    float saveZone = recorder.recordedTime / 50f;
                    if (Mathf.Abs(axis) > 0.01f) {
                        this.clipStartTime = Mathf.Clamp(this.clipStartTime + axis * Time.unscaledDeltaTime, this.recorder.startTime, this.clipEndTime - saveZone);
                        SetPlaybackTime(clipStartTime);
                    }
                    if (Mathf.Abs(axis2) > 0.01f) {
                        this.clipEndTime = Mathf.Clamp(this.clipEndTime + axis2 * Time.unscaledDeltaTime, this.clipStartTime + saveZone, this.recorder.endTime);
                        SetPlaybackTime(clipEndTime);
                    }
                }

                //TimeScaleManipulation
                float f = PlayerController.Instance.inputController.player.GetAxis("RT") - PlayerController.Instance.inputController.player.GetAxis("LT");
                if ((double)Mathf.Abs(f) > 0.001) {
                    this.timeScaleAddend = f;
                } else {
                    this.timeScaleAddend = 0f;
                }

                //JumpByTime
                float dpadX = PlayerController.Instance.inputController.player.GetAxis("DPadX");
                if (Mathf.Abs(dpadX) > 0.3f) {
                    if (Time.unscaledTime - Mathf.Abs(lastDpadTick) > Main.settings.DpadTickRate || Mathf.Sign(dpadX) != Mathf.Sign(lastDpadTick)) {
                        //KeyFrame keyFrame = this.cameraController.FindNextKeyFrame(this.playbackTime, dpadX < 0f);
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
            }
        }

        public void JumpByTime(float deltaTime, bool stopAtMarker) {
            if (stopAtMarker) {
                KeyFrame keyFrame = this.cameraController.SearchKeyFrameInRange(playbackTime, playbackTime + deltaTime);
                if (keyFrame != null) {
                    this.SetPlaybackTime(keyFrame.time);
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

        #region GUI
        public void OnGUI() {
            if (CurrentState != ReplayState.PLAYBACK) { return; }
            if (this.guiHidden) {
                if (Main.settings.showLogo) {
                    GUI.Box(ReplaySkin.DefaultSkin.logoRect, "", ReplaySkin.DefaultSkin.kiwiLogoStyle);
                }
                return;
            }
            if (Main.settings.showControllsHelp) {
                DrawControllsGUI();
            }

            DrawButtons();

            if (clipEditMode) {
                DrawClipEditSliders();
            }
            DrawTimeLineSliders();
            DrawKeyFrameMarkers();
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

            if (GUILayout.Button(menu.enabled ? "Hide" : "Show" + " menu")) {
                menu.Toggle();
            }

            if (GUILayout.Button("Show Help (" + (Main.settings.showControllsHelp ? "ON" : "OFF") + ")")) {
                Main.settings.showControllsHelp = !Main.settings.showControllsHelp;
                Main.settings.Save(Main.modEntry);
            }

            //showControllsHelp Toggle
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(ReplaySkin.DefaultSkin.timeScaleRect);
            GUILayout.BeginHorizontal();

            DrawInfoGUI();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void DrawInfoGUI() {
            //TimeScale
            GUILayout.Label(String.Format("{0:0.0}s /{1:0.0}s | Speed: {2:0.0}s/s", displayedPlaybackTime, recorder.recordedTime, playbackSpeed), ReplaySkin.DefaultSkin.fontMed, GUILayout.Width(200));
            float value = GUILayout.HorizontalSlider(playbackSpeed, 0f, 2f, ReplaySkin.DefaultSkin.timeScaleSliderStyle, GUI.skin.horizontalSliderThumb, GUILayout.MinWidth(300), GUILayout.MaxWidth(500));
            playbackSpeed = Mathf.Round(value * 10f) / 10f;

            GUILayout.FlexibleSpace();

            //FOV and FocusOffsetY
            float focalLength = Camera.FOVToFocalLength(cameraController.camera.fieldOfView, Main.settings.CameraSensorSize / 1000f) * 1000f;
            GUILayout.Label(String.Format("FOV: {0:0.0} ≡ Focal length: {1:0.00}mm, SensorSize of {2:0.00} mm", cameraController.camera.fieldOfView, focalLength, Main.settings.CameraSensorSize));
            GUILayout.Label("Focus y-offset: " + cameraController.FocusOffsetY + "m");

            GUILayout.FlexibleSpace();

            //Camera Mode
            GUILayout.Label("Camera-Mode: " + Enum.GetName(typeof(CameraMode), cameraController.mode));
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

            float st = Mathf.Clamp(GUI.HorizontalSlider(clipRects[0], this.clipStartTime, this.recorder.startTime, this.clipEndTime - saveZone, ReplaySkin.DefaultSkin.transparentSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle), this.recorder.startTime, this.clipEndTime);
            if (st != this.clipStartTime) {
                this.clipStartTime = st;
                SetPlaybackTime(clipStartTime);
            }

            float et = Mathf.Clamp(GUI.HorizontalSlider(clipRects[1], this.clipEndTime, this.clipStartTime + saveZone, this.recorder.endTime, ReplaySkin.DefaultSkin.transparentSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle), this.clipStartTime, this.recorder.endTime);
            if (et != this.clipEndTime) {
                this.clipEndTime = et;
                SetPlaybackTime(clipEndTime);
            }

            GUI.Box(clipRects[2], "", ReplaySkin.DefaultSkin.clipBoxStyle);
            GUI.Box(clipStartRect, "", ReplaySkin.DefaultSkin.clipCutStyle);
            GUI.Box(clipEndRect, "", ReplaySkin.DefaultSkin.clipCutStyle);
        }
        private void DrawTimeLineSliders() {

            float pTime = GUI.HorizontalSlider(ReplaySkin.DefaultSkin.sliderRect, this.playbackTime, this.recorder.startTime, this.recorder.endTime, ReplaySkin.DefaultSkin.clipSliderStyle, ReplaySkin.DefaultSkin.sliderThumbStyle);
            if (Mathf.Abs(pTime - this.playbackTime) > 1E-05f) {
                this.SetPlaybackTime(pTime);
            }

        }
        private void DrawKeyFrameMarkers() {
            int i = 0;
            foreach (KeyFrame keyFrame in this.cameraController.keyFrames) {
                float t = (keyFrame.time - this.recorder.startTime) / (this.recorder.endTime - this.recorder.startTime);
                Color textColor = (keyFrame is FreeCameraKeyFrame) ? Color.blue : ((keyFrame is OrbitCameraKeyFrame) ? Color.red : Color.green);
                ReplaySkin.DefaultSkin.markerStyle.normal.textColor = textColor;
                Rect markerRect = ReplaySkin.DefaultSkin.MarkerRectForNormT(t);
                int index = i;
                GUIHelper.DraggableArea(
                    "keyframe" + keyFrame.GetHashCode(),
                    markerRect,
                    delegate (out bool beginDrag) {  //GUI Draw
                        GUILayout.BeginVertical();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("|", ReplaySkin.DefaultSkin.markerStyle, GUILayout.ExpandHeight(true));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        beginDrag = GUILayout.RepeatButton("O", ReplaySkin.DefaultSkin.markerStyle);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.FlexibleSpace();
                        GUILayout.EndVertical();
                    },
                    () => SetPlaybackTime(keyFrame.time),
                    delegate (ref Rect r) {  //OnDragUpdate
                        float xmin = ReplaySkin.DefaultSkin.sliderRect.xMin + (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
                        float xmax = ReplaySkin.DefaultSkin.sliderRect.xMax - (float)ReplaySkin.DefaultSkin.sliderPadding / 2f;
                        r.center = new Vector2(Mathf.Clamp(r.center.x, xmin, xmax), markerRect.center.y);
                        float time = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
                        keyFrame.ApplyTo(cameraController.camera);
                        SetPlaybackTime(time);
                    },
                    delegate (Rect r) {  //OnDrop
                        float time = this.recorder.startTime + ReplaySkin.DefaultSkin.NormTForMarkerRect(r) * (this.recorder.endTime - this.recorder.startTime);
                        keyFrame.Update(cameraController.camera.transform, time);
                        cameraController.cameraCurve.DeleteCurveKeys(index);
                        keyFrame.AddKeyframes(cameraController.cameraCurve);
                    },
                    this,
                    true);

                //TODO: Finish Draggable KeyFrames
                //TODO: Update Curves on KeyFrame move
                i++;
            }
        }
        public void ReplayControllsWindow(int id) {
            GUILayout.BeginVertical();
            float y = 20f;
            GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label("Camera-Mode: " + Enum.GetName(typeof(CameraMode), cameraController.mode));
            GUILayout.Space(20);

            //legend
            GUILayout.BeginHorizontal();
            GUILayout.Label("ControllName");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Keyboard", ReplaySkin.DefaultSkin.helpTextKeyStyle);
            GUILayout.Label("Xbox", ReplaySkin.DefaultSkin.helpTextXboxStyle);
            GUILayout.Label("PS4", ReplaySkin.DefaultSkin.helpTextPs4Style);
            GUILayout.Label("All Controllers", ReplaySkin.DefaultSkin.helpTextControllerStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            DrawControllGUI("Use KeyFrame Animation (" + (cameraController.CamFollowKeyFrames ? "On" : "Off") + ")", "", "select", "share");
            DrawControllGUI("Show/Hide GUI", "Return", "RS", "R3");
            DrawControllGUI("Change Mode", "M", "Y", "\u25B3");
            DrawControllGUI("Add KeyFrame", "K", "X", "\u25A1");
            DrawControllGUI("Delete KeyFrame", "Delete", "Hold X", "Hold \u25A1");
            DrawControllGUI(String.Format("DPadX: Jump to next KeyFrame or max {0:0.#} s", Main.settings.PlaybackTimeJumpDelta), "Arrows", "DPadX", "DPadX");
            DrawControllGUI("Change Start of clip", "", "LB + LeftStickX", "LB + LeftStickX");
            DrawControllGUI("Change End of clip", "", "LB + RightStickX", "LB + RightStickX");

            switch (cameraController.mode) {
                case CameraMode.Free:
                    DrawControllGUI("Move(xz)", "", "LeftStick", "LeftStick");
                    DrawControllGUI("Move(y)", "", "DpadY", "DpadY");
                    DrawControllGUI("Rotate", "", "RightStick", "RightStick");
                    DrawControllGUI("Roll", "", "RB + RightStickX", "RB + RightStickX");
                    break;
                case CameraMode.Orbit:
                    DrawControllGUI("Orbit around Skater", "", "LeftStickX + RightStickY", "LeftStickX + RightStickY");
                    DrawControllGUI("Change Orbit Radius", "", "LeftStickY", "LeftStickY");
                    DrawControllGUI("Change Focus Offset", "", "RB + RightStickY", "RB + RightStickY");
                    break;
                case CameraMode.Tripod:
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

            GUILayout.Label(keyControll, ReplaySkin.DefaultSkin.helpTextKeyStyle);
            if (xboxControll == ps4Controll) {
                GUILayout.Label(xboxControll, ReplaySkin.DefaultSkin.helpTextControllerStyle);
            } else {
                GUILayout.Label(xboxControll, ReplaySkin.DefaultSkin.helpTextXboxStyle);
                GUILayout.Label(ps4Controll, ReplaySkin.DefaultSkin.helpTextPs4Style);
            }
            GUILayout.EndHorizontal();
        }
        #endregion
    }

}
