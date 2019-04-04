using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {

    public enum CameraMode {
        Free,
        Orbit,
        Tripod
    }
    public class ReplayCameraController : MonoBehaviour {

        private Transform cameraTransform;
        public Camera camera;
        public CameraMode mode;
        
        public float RotateSpeed { get { return Main.settings.RotateSpeed; } }
        public float TranslationSpeed { get { return Main.settings.TranslationSpeed; } }
        public float OrbitMoveSpeed { get { return Main.settings.OrbitMoveSpeed; } }
        public float FOVChangeSpeed { get { return Main.settings.FOVChangeSpeed; } }
        
        public List<KeyFrame> keyFrames;
        public CameraCurve cameraCurve;
        public bool CamFollowKeyFrames;

        public float FocusOffsetY;

        private Vector3Radial orbitRadialCoord;
        private float defaultCameraFOV;
        private ReplayManager manager;
        private Transform cameraParent;
        private const float KeyFrameDeleteTolerance = 0.1f;
        private float xDownTime;

        #region From Unity called Functions
        public void Awake() {
            this.manager = base.GetComponent<ReplayManager>();
            this.cameraTransform = PlayerController.Instance.cameraController._actualCam;
            this.camera = cameraTransform.GetComponent<Camera>();
            this.mode = CameraMode.Orbit;
            this.keyFrames = new List<KeyFrame>();
            this.orbitRadialCoord = new Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
            this.FocusOffsetY = 0f;
            this.cameraCurve = new CameraCurve();
        }

        public void Update() {
            if (ReplayManager.CurrentState != ReplayState.Playback)
                return;
            this.InputModeChange();
            this.InputKeyFrameControll();
            if (this.CamFollowKeyFrames) {
                this.EvaluateKeyFrames();
                return;
            }
            bool RBPressed = PlayerController.Instance.inputController.player.GetButton("RB");
            switch (this.mode) {
                case CameraMode.Free:
                    if (RBPressed) {
                        InputRollRotation();
                        InputCameraFOV();
                    } else {
                        this.InputFreePosition(true, false);
                        this.InputFreeRotation();
                    }
                    break;
                case CameraMode.Orbit:
                    if (RBPressed) {
                        InputFocusOffsetY();
                        InputCameraFOV();
                    } else {
                        this.InputOrbitMode();
                    }
                    this.cameraTransform.position =
                        PlayerController.Instance.skaterController.skaterTransform.position
                        + FocusOffsetY * Vector3.up
                        + this.orbitRadialCoord.cartesianCoords;
                    this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + FocusOffsetY * Vector3.up, Vector3.up);
                    break;
                case CameraMode.Tripod:
                    if (RBPressed) {
                        InputFocusOffsetY();
                        InputCameraFOV();
                    } else {
                        this.InputFreePosition(true, true);
                    }
                    this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + FocusOffsetY * Vector3.up, Vector3.up);
                    break;
            }
        }

        internal void LoadKeyFrames(IEnumerable<SerializableKeyFrame> cameraKeyFrames) {
            cameraCurve.Clear();
            keyFrames = new List<KeyFrame>(cameraKeyFrames.Select(k => k.GetKeyFrame(cameraCurve)));
            cameraCurve.CalculateCurveControlPoints();
        }

        #endregion

        #region Input
        private void InputCameraFOV() {
            float lsY = -PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
            if (Mathf.Abs(lsY) > 0.01) {
                camera.fieldOfView += lsY * FOVChangeSpeed * Time.unscaledDeltaTime;
            }
        }
        private void InputFocusOffsetY() {
            FocusOffsetY += PlayerController.Instance.inputController.player.GetAxis("RightStickY") * TranslationSpeed * Time.unscaledDeltaTime;
        }
        private void InputOrbitMode() {
            float axis = -PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
            float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
            PlayerController.Instance.inputController.player.GetAxis("RightStickX");
            float axis3 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
            this.orbitRadialCoord.phi = Mathf.Repeat(this.orbitRadialCoord.phi + axis * this.OrbitMoveSpeed / this.orbitRadialCoord.radius * Time.unscaledDeltaTime, 6.28318548f);
            this.orbitRadialCoord.theta = Mathf.Clamp(this.orbitRadialCoord.theta - axis3 * this.OrbitMoveSpeed / this.orbitRadialCoord.radius * Time.unscaledDeltaTime, 0.04f, 3.1f);
            this.orbitRadialCoord.radius = Mathf.Clamp(this.orbitRadialCoord.radius - axis2 * this.TranslationSpeed * Time.unscaledDeltaTime, 0.5f, 100f);
        }

        private void InputFreePosition(bool useDpadYForY = true, bool useRightStickForY = true) {
            float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
            float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
            float num = 0f;
            if (useDpadYForY) {
                num += PlayerController.Instance.inputController.player.GetAxis("DPadY") * TranslationSpeed * Time.unscaledDeltaTime;
            }
            if (useRightStickForY) {
                num += PlayerController.Instance.inputController.player.GetAxis("RightStickY") * TranslationSpeed * Time.unscaledDeltaTime;
            }
            Vector3 point = new Vector3(axis, num, axis2);
            Vector3 a = Quaternion.Euler(0f, this.cameraTransform.eulerAngles.y, 0f) * point;
            this.cameraTransform.position += a * this.TranslationSpeed * Time.unscaledDeltaTime;
        }

        private void InputFreeRotation() {
            float stickX = PlayerController.Instance.inputController.player.GetAxis("RightStickX");
            float stickY = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
            this.cameraTransform.transform.rotation = Quaternion.AngleAxis(stickY * this.RotateSpeed * Time.unscaledDeltaTime, Vector3.ProjectOnPlane(-cameraTransform.right, Vector3.up)) * cameraTransform.rotation;
            this.cameraTransform.transform.rotation = Quaternion.AngleAxis(stickX * this.RotateSpeed * Time.unscaledDeltaTime, Vector3.up) * cameraTransform.rotation;
        }
        private void InputRollRotation() {
            float stickX = PlayerController.Instance.inputController.player.GetAxis("RightStickX");
            this.cameraTransform.transform.rotation = Quaternion.AngleAxis(stickX * this.RotateSpeed * Time.unscaledDeltaTime, cameraTransform.forward) * cameraTransform.rotation;
        }

        private void InputModeChange() {
            if (PlayerController.Instance.inputController.player.GetButtonDown("Y") || Input.GetKeyDown(KeyCode.M)) {
                this.mode = ((this.mode >= CameraMode.Tripod) ? CameraMode.Free : (this.mode + 1));
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("Select")) {
                this.CamFollowKeyFrames = !this.CamFollowKeyFrames;
            }
        }

        private void InputKeyFrameControll() {
            if (PlayerController.Instance.inputController.player.GetButtonDown("X")) {
                this.xDownTime = Time.unscaledTime;
            }
            if (Input.GetKeyDown(KeyCode.Delete) || PlayerController.Instance.inputController.player.GetButton("X") && Time.unscaledTime - this.xDownTime > 1.5f) {
                this.DeleteKeyFrame();
            }
            if (Input.GetKeyDown(KeyCode.K) || PlayerController.Instance.inputController.player.GetButtonUp("X") && Time.unscaledTime - this.xDownTime < 0.5f) {
                this.AddKeyFrame(this.manager.playbackTime);
            }
        }
        #endregion

        #region KeyFrame Functions

        private void DeleteKeyFrame() {
            int index;
            if (!this.FindKeyFrameDeleteIndex(out index)) {
                return;
            }
            this.keyFrames.RemoveAt(index);
            cameraCurve.DeleteCurveKeys(index);
        }

        private bool FindKeyFrameDeleteIndex(out int index) {
            for (int i = 0; i < this.keyFrames.Count; i++) {
                if (Mathf.Abs(this.manager.playbackTime - this.keyFrames[i].time) < KeyFrameDeleteTolerance) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public void EvaluateKeyFrames() {
            if (this.keyFrames.Count <= 1) {
                return;
            }

            cameraCurve.Evaluate(this.manager.playbackTime).ApplyTo(this.cameraTransform);
        }

        private int FindLeftKeyFrameIndex() {
            if (this.manager.playbackTime < this.keyFrames[0].time) {
                return 0;
            }
            for (int i = 0; i < this.keyFrames.Count - 1; i++) {
                if (this.manager.playbackTime > this.keyFrames[i].time && this.manager.playbackTime < this.keyFrames[i + 1].time) {
                    return i;
                }
            }
            return this.keyFrames.Count - 2;
        }

        private void AddKeyFrame(float time) {
            int index = this.FindKeyFrameInsertIndex(time);
            KeyFrame item;
            switch (this.mode) {
                case CameraMode.Free:
                    item = new FreeCameraKeyFrame(this.cameraTransform, camera.fieldOfView, time, cameraCurve);
                    break;
                case CameraMode.Orbit:
                    item = new OrbitCameraKeyFrame(this.orbitRadialCoord, FocusOffsetY, camera.fieldOfView, time, cameraCurve);
                    break;
                case CameraMode.Tripod:
                    item = new TripodCameraKeyFrame(this.cameraTransform, FocusOffsetY, camera.fieldOfView, time, cameraCurve);
                    break;
                default:
                    return;
            }
            this.keyFrames.Insert(index, item);
        }

        private int FindKeyFrameInsertIndex(float time) {
            if (this.keyFrames.Count == 0) {
                return 0;
            }
            if (time < this.keyFrames[0].time) {
                return 0;
            }
            if (this.keyFrames.Count == 1) {
                return 1;
            }
            for (int i = 0; i < this.keyFrames.Count - 1; i++) {
                if (time > this.keyFrames[i].time && time < this.keyFrames[i + 1].time) {
                    return i + 1;
                }
            }
            return this.keyFrames.Count;
        }
        
        public KeyFrame FindNextKeyFrame(float time, bool left) {
            if (this.keyFrames.Count == 0) {
                return null;
            }
            if (left) {
                foreach (KeyFrame ks in this.keyFrames) {
                    if (ks.time < time) {
                        return ks;
                    }
                }
            } else {
                foreach (KeyFrame ks in this.keyFrames) {
                    if (ks.time > time) {
                        return ks;
                    }
                }
            }
            return null;
        }
        
        public KeyFrame SearchKeyFrameInRange(float start, float end) {
            if (this.keyFrames.Count == 0) {
                return null;
            }
            if (start < end) {
                var kfs = from t in this.keyFrames
                          where t.time > start && t.time <= end
                          orderby t.time ascending
                          select t;
                if (kfs.Count() == 0) return null;
                return kfs.First();
            } else {
                var ks = from t in this.keyFrames
                         where t.time < start && t.time >= end
                         orderby t.time descending
                         select t;
                if (ks.Count() == 0) return null;
                return ks.First();
            }
        }
        #endregion

        public void OnStartReplayEditor() {
            while (this.keyFrames.Count > 0 && this.keyFrames[0].time < this.manager.recorder.startTime) {
                this.keyFrames.RemoveAt(0);
            }
            base.enabled = true;
            if (this.mode == CameraMode.Orbit) {
                this.orbitRadialCoord = new Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
            }
            this.cameraParent = this.cameraTransform.parent;
            PlayerController.Instance.cameraController._actualCam.SetParent(null);
            CameraMovementController cameraMovementController = UnityEngine.Object.FindObjectOfType<CameraMovementController>();
            if (cameraMovementController) {
                cameraMovementController.enabled = false;
            }
            defaultCameraFOV = camera.fieldOfView;
        }

        public void OnExitReplayEditor() {
            base.enabled = false;
            PlayerController.Instance.cameraController._actualCam.SetParent(this.cameraParent);
            CameraMovementController cameraMovementController = UnityEngine.Object.FindObjectOfType<CameraMovementController>();
            if (cameraMovementController) {
                cameraMovementController.enabled = true;
            }
            camera.fieldOfView = defaultCameraFOV;
        }


        private void SwitchModeTo(CameraMode newValue) {
            CameraMode cameraMode = this.mode;
            if (newValue == cameraMode) {
                return;
            }
            this.mode = newValue;
            if (newValue == CameraMode.Orbit) {
                this.orbitRadialCoord = new Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.transform.position);
            }
        }
    }
}