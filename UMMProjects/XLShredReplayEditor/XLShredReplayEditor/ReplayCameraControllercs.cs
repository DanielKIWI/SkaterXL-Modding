using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {
    public class ReplayCameraController : MonoBehaviour {

        #region From Unity called Functions
        public void Awake() {
            this.manager = base.GetComponent<ReplayManager>();
            this.cameraTransform = PlayerController.Instance.cameraController._actualCam;
            camera = cameraTransform.GetComponent<Camera>();
            this.FreeMoveSpeed = 5f;
            this.OrbitMoveSpeed = 5f;
            this.RotateSpeed = 20f;
            this.FOVChangeSpeed = 20f;
            this.mode = ReplayCameraController.CameraMode.Orbit;
            this.keyStones = new List<ReplayCameraController.KeyStone>();
            this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
        }

        public void Update() {
            this.InputModeChange();
            this.InputKeyStoneControll();
            if (this.CamFollowKeyStones) {
                this.LerpKeyStones();
                return;
            }
            if (PlayerController.Instance.inputController.player.GetButton("RB")) {
                InputCameraFOV();
            } else {
                switch (this.mode) {
                    case ReplayCameraController.CameraMode.Free:
                        this.InputFreePosition(true, false);
                        this.InputFreeRotation();
                        break;
                    case ReplayCameraController.CameraMode.Orbit:
                        this.InputOrbitMode();
                        this.cameraTransform.position = PlayerController.Instance.skaterController.skaterTransform.position + this.orbitRadialCoord.cartesianCoords;
                        this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
                        break;
                    case ReplayCameraController.CameraMode.Tripod:
                        this.InputFreePosition(true, true);
                        this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
                        break;
                }
            }
        }

        public void OnGUI() {
            if (this.manager.guiHidden) {
                return;
            }
            float w = 300f;
            float x = (float)Screen.width - w - 20f;
            float boxY = (float)Screen.height / 2f - 100f;
            float y = 20f;
            GUI.skin.label.normal.textColor = Color.white;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "CamMode: " + Enum.GetName(typeof(ReplayCameraController.CameraMode), this.mode));
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "Back: Enable KeyStone Animation (" + (this.CamFollowKeyStones ? "On" : "Off") + ")");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "RightStick: Show/Hide GUI");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "Y: Change Mode");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "X: Add KeyStone");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "Hold X: Delete KeyStone");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "DPadX: Jump to next KeyStone or max 5s");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "LB + LeftStickX Change Start of clip");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "LB + RightStickX Change End of clip");
            y += 20f;
            switch (this.mode) {
                case ReplayCameraController.CameraMode.Free:
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "LeftStick: Move(xz)");
                    y += 20f;
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "DpadY: Move(y)");
                    y += 20f;
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "RightStick: Rotate");
                    y += 20f;
                    break;
                case ReplayCameraController.CameraMode.Orbit:
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "LeftStickX + RightStickY: Orbit around Skater");
                    y += 20f;
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "LeftStickY: Change Orbit Radius");
                    y += 20f;
                    break;
                case ReplayCameraController.CameraMode.Tripod:
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "LeftStick: Move(xz)");
                    y += 20f;
                    GUI.Label(new Rect(x, boxY + y, w, 20f), "DpadY or RightStickY: Move(y)");
                    y += 20f;
                    break;
            }
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "RB + LeftStickY Change camera FOV");
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "Free Move Speed");
            y += 20f;
            FreeMoveSpeed = GUI.HorizontalSlider(new Rect(x, boxY + y, w, 20f), FreeMoveSpeed, 0, 10);
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "Free Rotate Speed");
            y += 20f;
            RotateSpeed = GUI.HorizontalSlider(new Rect(x, boxY + y, w, 20f), RotateSpeed, 0, 40);
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "Orbit Move Speed");
            y += 20f;
            OrbitMoveSpeed = GUI.HorizontalSlider(new Rect(x, boxY + y, w, 20f), OrbitMoveSpeed, 0, 10);
            y += 20f;
            GUI.Label(new Rect(x, boxY + y, w, 20f), "FOV Change Speed");
            y += 20f;
            FOVChangeSpeed = GUI.HorizontalSlider(new Rect(x, boxY + y, w, 20f), FOVChangeSpeed, 0, 40);
            y += 20f;
            GUI.Box(new Rect(x, boxY, w, y), "Camera");
        }
        #endregion

        #region Input
        private void InputCameraFOV() {
            float dpadX = -PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
            if (Mathf.Abs(dpadX) > 0.01) {
                camera.fieldOfView += dpadX * FOVChangeSpeed * Time.unscaledDeltaTime;
            }
        }
        private void InputOrbitMode() {
            float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
            float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
            PlayerController.Instance.inputController.player.GetAxis("RightStickX");
            float axis3 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
            this.orbitRadialCoord.phi = Mathf.Repeat(this.orbitRadialCoord.phi + axis * this.OrbitMoveSpeed / this.orbitRadialCoord.radius * Time.unscaledDeltaTime, 6.28318548f);
            this.orbitRadialCoord.theta = Mathf.Clamp(this.orbitRadialCoord.theta - axis3 * this.OrbitMoveSpeed / this.orbitRadialCoord.radius * Time.unscaledDeltaTime, 0.04f, 3.1f);
            this.orbitRadialCoord.radius = Mathf.Clamp(this.orbitRadialCoord.radius - axis2 * this.FreeMoveSpeed * Time.unscaledDeltaTime, 0.5f, 100f);
        }

        private void InputFreePosition(bool useDpadYForY = true, bool useRightStickForY = true) {
            float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
            float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
            float num = 0f;
            if (useDpadYForY) {
                num += PlayerController.Instance.inputController.player.GetAxis("DPadY") * FreeMoveSpeed * Time.unscaledDeltaTime;
            }
            if (useRightStickForY) {
                num += PlayerController.Instance.inputController.player.GetAxis("RightStickY") * FreeMoveSpeed * Time.unscaledDeltaTime;
            }
            Vector3 point = new Vector3(axis, num, axis2);
            Vector3 a = Quaternion.Euler(0f, this.cameraTransform.eulerAngles.y, 0f) * point;
            this.cameraTransform.position += a * this.FreeMoveSpeed * Time.unscaledDeltaTime;
        }

        private void InputFreeRotation() {
            float axis = PlayerController.Instance.inputController.player.GetAxis("RightStickX");
            float axis2 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
            this.cameraTransform.rotation.SetLookRotation(this.cameraTransform.forward, Vector3.up);
            this.cameraTransform.Rotate(-axis2 * this.RotateSpeed * Time.unscaledDeltaTime, axis * this.RotateSpeed * Time.unscaledDeltaTime, 0f);
        }

        private void InputModeChange() {
            if (PlayerController.Instance.inputController.player.GetButtonDown("Y")) {
                this.mode = ((this.mode >= ReplayCameraController.CameraMode.Tripod) ? ReplayCameraController.CameraMode.Free : (this.mode + 1));
            }
            if (PlayerController.Instance.inputController.player.GetButtonDown("Select")) {
                this.CamFollowKeyStones = !this.CamFollowKeyStones;
            }
        }

        private void InputKeyStoneControll() {
            if (PlayerController.Instance.inputController.player.GetButtonDown("X")) {
                this.xDownTime = Time.unscaledTime;
            }
            if (PlayerController.Instance.inputController.player.GetButton("X") && Time.unscaledTime - this.xDownTime > 1.5f) {
                this.DeleteKeyStone();
            }
            if (PlayerController.Instance.inputController.player.GetButtonUp("X") && Time.unscaledTime - this.xDownTime < 1.5f) {
                this.AddKeyStone(this.manager.playbackTime);
            }
        }
        #endregion

        #region KeyStone Functions

        private void DeleteKeyStone() {
            int index;
            if (!this.FindKeyStoneDeleteIndex(out index)) {
                return;
            }
            this.keyStones.RemoveAt(index);
        }

        private bool FindKeyStoneDeleteIndex(out int index) {
            for (int i = 0; i < this.keyStones.Count; i++) {
                if (Mathf.Abs(this.manager.playbackTime - this.keyStones[i].time) < this.keyStoneDeleteTolerance) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public void LerpKeyStones() {
            if (this.keyStones.Count <= 0) {
                return;
            }
            if (this.keyStones.Count == 1) {
                this.keyStones[0].ApplyTo(this.cameraTransform);
                return;
            }
            int num = this.FindLeftKeyStoneIndex();
            ReplayCameraController.KeyStone keyStone = this.keyStones[num];
            ReplayCameraController.KeyStone keyStone2 = this.keyStones[num + 1];
            if (keyStone is ReplayCameraController.FreeCameraKeyStone || keyStone2 is ReplayCameraController.FreeCameraKeyStone) {
                ReplayCameraController.FreeCameraKeyStone.Lerp(keyStone, keyStone2, this.manager.playbackTime).ApplyTo(this.cameraTransform);
            }
            if (keyStone is ReplayCameraController.TripodCameraKeyStone || keyStone2 is ReplayCameraController.TripodCameraKeyStone) {
                ReplayCameraController.TripodCameraKeyStone.Lerp(keyStone, keyStone2, this.manager.playbackTime).ApplyTo(this.cameraTransform);
            }
            if (keyStone is ReplayCameraController.OrbitCameraKeyStone && keyStone2 is ReplayCameraController.OrbitCameraKeyStone) {
                ReplayCameraController.OrbitCameraKeyStone.Lerp(keyStone as ReplayCameraController.OrbitCameraKeyStone, keyStone2 as ReplayCameraController.OrbitCameraKeyStone, this.manager.playbackTime).ApplyTo(this.cameraTransform);
            }
        }

        private int FindLeftKeyStoneIndex() {
            if (this.manager.playbackTime < this.keyStones[0].time) {
                return 0;
            }
            for (int i = 0; i < this.keyStones.Count - 1; i++) {
                if (this.manager.playbackTime > this.keyStones[i].time && this.manager.playbackTime < this.keyStones[i + 1].time) {
                    return i;
                }
            }
            return this.keyStones.Count - 2;
        }

        private void AddKeyStone(float time) {
            int index = this.FindKeyStoneInsertIndex(time);
            ReplayCameraController.KeyStone item;
            switch (this.mode) {
                case ReplayCameraController.CameraMode.Free:
                    item = new ReplayCameraController.FreeCameraKeyStone(this.cameraTransform, cameraFOV, time);
                    break;
                case ReplayCameraController.CameraMode.Orbit:
                    item = new ReplayCameraController.OrbitCameraKeyStone(this.orbitRadialCoord, cameraFOV, time);
                    break;
                case ReplayCameraController.CameraMode.Tripod:
                    item = new ReplayCameraController.TripodCameraKeyStone(this.cameraTransform, cameraFOV, time);
                    break;
                default:
                    return;
            }
            this.keyStones.Insert(index, item);
        }

        private int FindKeyStoneInsertIndex(float time) {
            if (this.keyStones.Count == 0) {
                return 0;
            }
            if (time < this.keyStones[0].time) {
                return 0;
            }
            if (this.keyStones.Count == 1) {
                return 1;
            }
            for (int i = 0; i < this.keyStones.Count - 1; i++) {
                if (time > this.keyStones[i].time && time < this.keyStones[i + 1].time) {
                    return i + 1;
                }
            }
            return this.keyStones.Count;
        }



        public ReplayCameraController.KeyStone FindNextKeyStone(float time, bool left) {
            if (this.keyStones.Count == 0) {
                return null;
            }
            if (left) {
                foreach (ReplayCameraController.KeyStone ks in this.keyStones) {
                    if (ks.time < time) {
                        return ks;
                    }
                }
            } else {
                foreach (ReplayCameraController.KeyStone ks in this.keyStones) {
                    if (ks.time > time) {
                        return ks;
                    }
                }
            }
            return null;
        }
        #endregion

        public void OnStartReplayEditor() {
            while (this.keyStones.Count > 0 && this.keyStones[0].time < this.manager.recorder.startTime) {
                this.keyStones.RemoveAt(0);
            }
            base.enabled = true;
            if (this.mode == ReplayCameraController.CameraMode.Orbit) {
                this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
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


        private void SwitchModeTo(ReplayCameraController.CameraMode newValue) {
            ReplayCameraController.CameraMode cameraMode = this.mode;
            if (newValue == cameraMode) {
                return;
            }
            this.mode = newValue;
            if (newValue == ReplayCameraController.CameraMode.Orbit) {
                this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.transform.position);
            }
        }

        private Transform cameraTransform;

        private Camera camera;

        public ReplayCameraController.CameraMode mode;

        public Vector3 lookDirection;

        public ReplayCameraController.Vector3Radial orbitRadialCoord;

        public float RotateSpeed;

        public float FreeMoveSpeed;

        public float OrbitMoveSpeed;
        public float FOVChangeSpeed;
        private float cameraFOV;
        private float defaultCameraFOV;

        private bool dpadCentered = true;

        private float xDownTime;

        private float keyStoneDeleteTolerance = 0.1f;

        public List<ReplayCameraController.KeyStone> keyStones;

        private Transform cameraParent;

        public bool CamFollowKeyStones;

        private ReplayManager manager;

        public enum CameraMode {
            Free,
            Orbit,
            Tripod
        }

        public struct Vector3Radial {
            public Vector3Radial(float p, float t, float r) {
                this.phi = p;
                this.theta = t;
                this.radius = r;
            }

            public Vector3Radial(Vector3 source) {
                this.radius = source.magnitude;
                this.phi = Mathf.Atan2(source.x, source.z);
                this.theta = Mathf.Acos(source.y / source.magnitude);
            }

            public Vector3 cartesianCoords {
                get {
                    return new Vector3(this.radius * Mathf.Sin(this.theta) * Mathf.Sin(this.phi), this.radius * Mathf.Cos(this.theta), this.radius * Mathf.Sin(this.theta) * Mathf.Cos(this.phi));
                }
            }

            public static ReplayCameraController.Vector3Radial Lerp(ReplayCameraController.Vector3Radial l, ReplayCameraController.Vector3Radial r, float t) {
                return new ReplayCameraController.Vector3Radial(Mathf.LerpAngle(l.phi, r.phi, t), Mathf.LerpAngle(l.theta, r.theta, t), Mathf.Lerp(l.radius, r.radius, t));
            }

            public float phi;

            public float theta;

            public float radius;
        }

        [Serializable]
        public class KeyStone {
            public virtual void ApplyTo(Transform t) {
                t.position = this.position;
                t.rotation = this.rotation;
            }

            public float time;

            public Vector3 position;

            public Quaternion rotation;
            public float cameraFOV;
        }

        [Serializable]
        public class FreeCameraKeyStone : ReplayCameraController.KeyStone {
            public FreeCameraKeyStone(Transform cameraTransform, float fov, float t) {
                this.position = cameraTransform.position;
                this.rotation = cameraTransform.rotation;
                this.time = t;
                this.cameraFOV = fov;
            }

            public FreeCameraKeyStone(Vector3 p, Quaternion r, float fov, float t) {
                this.position = p;
                this.rotation = r;
                this.time = t;
                this.cameraFOV = fov;
            }

            public static ReplayCameraController.FreeCameraKeyStone Lerp(ReplayCameraController.KeyStone a, ReplayCameraController.KeyStone b, float time) {
                float t = (time - a.time) / (b.time - a.time);
                return new ReplayCameraController.FreeCameraKeyStone(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
            }

            public FreeCameraKeyStone(ReplayCameraController.KeyStone ks) {
                this.position = ks.position;
                this.rotation = ks.rotation;
                this.time = ks.time;
                this.cameraFOV = ks.cameraFOV;
            }
        }

        [Serializable]
        public class OrbitCameraKeyStone : ReplayCameraController.KeyStone {
            public OrbitCameraKeyStone(ReplayCameraController.Vector3Radial radialPos, float fov, float t) {
                this.radialPos = radialPos;
                this.position = PlayerController.Instance.skaterController.skaterTransform.position + radialPos.cartesianCoords;
                this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
                this.time = t;
                this.cameraFOV = fov;
            }
            public OrbitCameraKeyStone(Vector3 v, float fov, float t) {
                this.radialPos = new ReplayCameraController.Vector3Radial(v);
                this.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
                this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
                this.time = t;
                this.cameraFOV = fov;
            }

            public override void ApplyTo(Transform t) {
                t.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
                t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
            }

            public static ReplayCameraController.OrbitCameraKeyStone Lerp(ReplayCameraController.OrbitCameraKeyStone a, ReplayCameraController.OrbitCameraKeyStone b, float time) {
                float t = (time - a.time) / (b.time - a.time);
                return new ReplayCameraController.OrbitCameraKeyStone(ReplayCameraController.Vector3Radial.Lerp(a.radialPos, b.radialPos, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
            }


            public ReplayCameraController.Vector3Radial radialPos;
        }

        [Serializable]
        public class TripodCameraKeyStone : ReplayCameraController.KeyStone {
            public TripodCameraKeyStone(Vector3 p, float fov, float t) {
                this.position = p;
                this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
                this.time = t;
                this.cameraFOV = fov;
            }
            public TripodCameraKeyStone(Transform cameraTransform, float fov, float t) {
                this.position = cameraTransform.position;
                this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
                this.time = t;
                this.cameraFOV = fov;
            }

            public override void ApplyTo(Transform t) {
                t.position = this.position;
                t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
            }

            public static ReplayCameraController.TripodCameraKeyStone Lerp(ReplayCameraController.KeyStone a, ReplayCameraController.KeyStone b, float time) {
                float t = (time - a.time) / (b.time - a.time);
                return new ReplayCameraController.TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
            }
        }
    }
}