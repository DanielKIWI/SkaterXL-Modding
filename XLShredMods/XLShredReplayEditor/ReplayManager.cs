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

    public static class ReplayStateExtension {
        public static bool IsMenuOpen(this ReplayState state) {
            switch (state) {
                case ReplayState.MainMenu:
                case ReplayState.SaveMenu:
                case ReplayState.LoadMenu:
                case ReplayState.SettingsMenu:
                case ReplayState.SavingReplay:
                case ReplayState.LoadingReplay:
                    return true;
            }
            return false;
        }
        public static bool CanBeChange(this ReplayState state) {
            switch (state) {
                case ReplayState.Recording:
                case ReplayState.SavingReplay:
                case ReplayState.LoadingReplay:
                case ReplayState.LoadingEditor:
                    return false;
            }
            return true;
        }
        public static bool NeedsCursor(this ReplayState state) {
            switch (state) {
                case ReplayState.SavingReplay:
                case ReplayState.LoadingReplay:
                case ReplayState.LoadingEditor:
                case ReplayState.Disabled:
                case ReplayState.Recording:
                    return false;
            }
            return true;
        }
    }

    public enum ReplayState {
        Disabled = 0, Recording = 1, Playback = 2,
        MainMenu = 10, SaveMenu = 11, LoadMenu = 12, SettingsMenu = 13,
        LoadingEditor = -1, SavingReplay = -2, LoadingReplay = -3
    }

    public class ReplayManager : MonoBehaviour {
        private static ReplayState _currentState = ReplayState.Disabled;
        public static ReplayState CurrentState {
            get { return _currentState; }
            set {
                if (_currentState == value) return;
                Main.modEntry.Logger.Log("Changed ReplayState to " + value.ToString());
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
        bool deleteKeyFramesSecurityCheck;
        private bool isPlaying;

        public float clipStartTime;
        public float clipEndTime;

        private float timeScaleAddend;
        private Animator[] disabledAnimators;

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
            this.menu = new ReplayEditorMenu();
            StateChangedEvent += this.menu.OnStateChanged;
            audioRecorder = PlayerController.Instance.skaterController.skaterTransform.gameObject.AddComponent<ReplayAudioRecorder>();
            audioRecorder.enabled = true;
        }

        public void Start() {
            ReplayManager.CurrentState = ReplayState.Recording;
            audioRecorder.StartRecording();
        }

        public void Destroy() {
            recorder.Destroy();
            audioRecorder.Destroy();
            ReplayManager.CurrentState = ReplayState.Disabled;
            StateChangedEvent -= this.menu.OnStateChanged;
            menu = null;
            Destroy(gameObject);
        }

        public IEnumerator StartReplayEditor() {
            ReplayManager.CurrentState = ReplayState.LoadingEditor;
            Main.modEntry.Logger.Log("Started Replay Editor. endTime(" + recorder.endTime + ") - startTime(" + recorder.startTime + ") = " + (recorder.endTime - recorder.startTime));

            //Disabling core Game Input and animation that would interfer
            SoundManager.Instance.deckSounds.MuteAll();
            PlayerController.Instance.cameraController.enabled = false;
            InputController.Instance.enabled = false;
            PlayerController.Instance.enabled = false;
            PlayerController.Instance.respawn.pin.gameObject.SetActive(false);

            disabledAnimators = PlayerController.Instance.GetComponentsInChildren<Animator>().Where(a => a.isActiveAndEnabled).ToArray();
            foreach (var anim in disabledAnimators) {
                anim.enabled = false;
            }

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
            ReplayManager.CurrentState = ReplayState.Playback;
            XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", true);
        }

        public void ExitReplayEditor() {
            try {
                ReplayManager.CurrentState = ReplayState.LoadingEditor;

                audioRecorder.StopPlayback();
                audioRecorder.StartRecording();
                cameraController.OnExitReplayEditor();
                recorder.OnExitReplayEditor();
                
                PlayerController.Instance.respawn.pin.gameObject.SetActive(true);
                PlayerController.Instance.cameraController.enabled = true;
                InputController.Instance.enabled = true;
                PlayerController.Instance.enabled = true;
                SoundManager.Instance.deckSounds.UnMuteAll();

                foreach (var anim in disabledAnimators) {
                    anim.enabled = true;
                }
                disabledAnimators = null;

                ModMenu.Instance.UnregisterTimeScaleTarget(Main.modId);
                Time.timeScale = 1f;

                ReplayManager.CurrentState = ReplayState.Recording;
                XLShredDataRegistry.SetData(Main.modId, "isReplayEditorActive", false);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public void OpenMenu() {
            if (CurrentState.CanBeChange())
                CurrentState = ReplayState.MainMenu;
        }

        public void CloseMenu() {
            if (CurrentState.CanBeChange())
                CurrentState = ReplayState.Playback;
        }

        public void ToggleMenu() {
            if (CurrentState.IsMenuOpen())
                CloseMenu();
            else
                OpenMenu();
        }

        public void Update() {
            CheckInput();
            if (CurrentState == ReplayState.Playback)
                PlaybackUpdate();
        }

        private void CheckInput() {
            if (CurrentState == ReplayState.Recording)
                CheckRecordingInput();
            if (CurrentState == ReplayState.Playback)
                CheckPlaybackInput();
            if (CurrentState.IsMenuOpen())
                menu.CheckInput(CurrentState);
        }

        private void PlaybackUpdate() {
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

        private void CheckRecordingInput() {
            if (CurrentState == ReplayState.Recording && (PlayerController.Instance.inputController.player.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) {
                StartCoroutine(StartReplayEditor());
                return;
            }
        }

        private void CheckPlaybackInput() {
            if (PlayerController.Instance.inputController.player.GetButtonDown("B") || Input.GetKeyDown(KeyCode.Escape)) {
                this.ExitReplayEditor();
                return;
            }

            if (CurrentState == ReplayState.Playback && (PlayerController.Instance.inputController.player.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) {
                OpenMenu();
            }

            if (PlayerController.Instance.inputController.player.GetButtonDown("Right Stick Button") || Input.GetKeyDown(KeyCode.H)) {
                this.guiHidden = !this.guiHidden;
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("A") || Input.GetKeyDown(KeyCode.Space)) {
                if (this.playbackTime == this.clipEndTime) {
                    this.playbackTime = this.clipStartTime;
                }
                this.isPlaying = !this.isPlaying;
            }

            //Save
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.S) && CurrentState.CanBeChange()) {
                CurrentState = ReplayState.SaveMenu;
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
            Main.modEntry.Logger.Log("Cutting clip from " + recorder.startTime + "-" + recorder.endTime + " to " + clipStartTime + "-" + clipEndTime);
            recorder.CutClip(clipStartTime, clipEndTime);
            cameraController.DeleteKeyFramesOutside(clipStartTime, clipEndTime);
        }

        #region GUI
        public void OnGUI() {
            if (CurrentState == ReplayState.Playback)
                DrawPlaybackEditorGUI();
            if (CurrentState.IsMenuOpen())
                menu.DrawGUI(CurrentState);
        }
        void DrawPlaybackEditorGUI() {
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
                if (GUILayout.Button("Edit Clip Length")) {
                    clipEditMode = true;
                }
            }

            if (deleteKeyFramesSecurityCheck) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("!Clear KeyFrames!")) {
                    deleteKeyFramesSecurityCheck = false;
                    cameraController.keyFrames.Clear();
                    cameraController.cameraCurve.Clear();
                }
                if (GUILayout.Button("Cancel")) {
                    deleteKeyFramesSecurityCheck = false;
                }
                GUILayout.EndHorizontal();
            } else {
                if (GUILayout.Button("Clear KeyFrames")) {
                    deleteKeyFramesSecurityCheck = true;
                }
            }

            if (GUILayout.Button(CurrentState.IsMenuOpen() ? "Hide" : "Show" + " menu")) {
                ToggleMenu();
            }
            if (GUILayout.Button("Refresh CameraCurve")) {
                cameraController.RefreshCameraCurve();
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
                        keyFrame.AddCurveKeys(cameraController.cameraCurve);
                    },
                    this,
                    true);
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
            DrawControllGUI("Open Menu", "Enter", "Start", "Start");
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
