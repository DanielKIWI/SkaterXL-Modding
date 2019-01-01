using System;
using System.Collections.Generic;
using UnityEngine;


namespace XLShredReplayEditor {

    // Token: 0x0200021E RID: 542
    public class ReplayCameraController : MonoBehaviour {
    // Token: 0x060016C7 RID: 5831 RVA: 0x00071834 File Offset: 0x0006FA34
    public void Awake() {
        this.manager = base.GetComponent<ReplayManager>();
        this.cameraTransform = PlayerController.Instance.cameraController._actualCam;
        this.FreeMoveSpeed = 0.12f;
        this.OrbitMoveSpeed = 0.12f;
        this.RotateSpeed = 0.5f;
        this.mode = ReplayCameraController.CameraMode.Orbit;
        this.keyStones = new List<ReplayCameraController.KeyStone>();
        this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
    }

    // Token: 0x060016C9 RID: 5833 RVA: 0x000718C4 File Offset: 0x0006FAC4
    public void Update() {
        this.InputModeChange();
        this.InputKeyStoneControll();
        if (this.CamFollowKeyStones) {
            this.LerpKeyStones();
            return;
        }
        switch (this.mode) {
            case ReplayCameraController.CameraMode.Free:
                this.InputFreePosition(true, false);
                this.InputFreeRotation();
                return;
            case ReplayCameraController.CameraMode.Orbit:
                this.InputOrbitMode();
                this.cameraTransform.position = PlayerController.Instance.skaterController.skaterTransform.position + this.orbitRadialCoord.cartesianCoords;
                this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
                return;
            case ReplayCameraController.CameraMode.Tripod:
                this.InputFreePosition(true, true);
                this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
                return;
            default:
                return;
        }
    }

    // Token: 0x060016CA RID: 5834 RVA: 0x00071994 File Offset: 0x0006FB94
    private void InputOrbitMode() {
        float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
        float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
        PlayerController.Instance.inputController.player.GetAxis("RightStickX");
        float axis3 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
        this.orbitRadialCoord.phi = Mathf.Repeat(this.orbitRadialCoord.phi + axis * this.OrbitMoveSpeed / this.orbitRadialCoord.radius, 6.28318548f);
        this.orbitRadialCoord.theta = Mathf.Clamp(this.orbitRadialCoord.theta - axis3 * this.OrbitMoveSpeed / this.orbitRadialCoord.radius, 0.04f, 3.1f);
        this.orbitRadialCoord.radius = Mathf.Clamp(this.orbitRadialCoord.radius - axis2 * this.FreeMoveSpeed, 0.5f, 100f);
    }

    // Token: 0x060016CB RID: 5835 RVA: 0x00071AA8 File Offset: 0x0006FCA8
    public void OnGUI() {
        if (this.manager.guiHidden) {
            return;
        }
        float num = 300f;
        float x = (float)Screen.width - num - 20f;
        float num2 = (float)Screen.height / 2f - 100f;
        float num3 = 20f;
        GUI.skin.label.normal.textColor = Color.white;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "CamMode: " + Enum.GetName(typeof(ReplayCameraController.CameraMode), this.mode));
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "Back: Enable KeyStone Animation (" + (this.CamFollowKeyStones ? "On" : "Off") + ")");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "RightStick: Show/Hide GUI");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "Y: Change Mode");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "X: Add KeyStone");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "Hold X: Delete KeyStone");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "DPadX: Jump to next KeyStone or max 5s");
        num3 += 20f;
        switch (this.mode) {
            case ReplayCameraController.CameraMode.Free:
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStick: Move(xz)");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "DpadY: Move(y)");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "RightStick: Rotate");
                num3 += 20f;
                break;
            case ReplayCameraController.CameraMode.Orbit:
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStickX + RightStickY: Orbit around Skater");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStickY: Change Orbit Radius");
                num3 += 20f;
                break;
            case ReplayCameraController.CameraMode.Tripod:
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStick: Move(xz)");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "DpadY or RightStickY: Move(y)");
                num3 += 20f;
                break;
        }
        GUI.Box(new Rect(x, num2, num, num3), "Camera");
    }

    // Token: 0x060016CC RID: 5836 RVA: 0x00071D50 File Offset: 0x0006FF50
    private void InputFreeRotation() {
        float axis = PlayerController.Instance.inputController.player.GetAxis("RightStickX");
        float axis2 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
        this.cameraTransform.rotation.SetLookRotation(this.cameraTransform.forward, Vector3.up);
        this.cameraTransform.Rotate(-axis2 * this.RotateSpeed, axis * this.RotateSpeed, 0f);
    }

    // Token: 0x060016CD RID: 5837 RVA: 0x00071DD8 File Offset: 0x0006FFD8
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

    // Token: 0x060016CE RID: 5838 RVA: 0x00071E2C File Offset: 0x0007002C
    private void InputModeChange() {
        if (PlayerController.Instance.inputController.player.GetButtonDown("Y")) {
            this.mode = ((this.mode >= ReplayCameraController.CameraMode.Tripod) ? ReplayCameraController.CameraMode.Free : (this.mode + 1));
        }
        if (PlayerController.Instance.inputController.player.GetButtonDown("Select")) {
            this.CamFollowKeyStones = !this.CamFollowKeyStones;
        }
    }

    // Token: 0x060016CF RID: 5839 RVA: 0x00071E98 File Offset: 0x00070098
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

    // Token: 0x060016D0 RID: 5840 RVA: 0x00071F40 File Offset: 0x00070140
    private void DeleteKeyStone() {
        int index;
        if (!this.FindKeyStoneDeleteIndex(out index)) {
            return;
        }
        this.keyStones.RemoveAt(index);
    }

    // Token: 0x060016D1 RID: 5841 RVA: 0x00071F64 File Offset: 0x00070164
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

    // Token: 0x060016D2 RID: 5842 RVA: 0x00071FBC File Offset: 0x000701BC
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

    // Token: 0x060016D3 RID: 5843 RVA: 0x000720B4 File Offset: 0x000702B4
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

    // Token: 0x060016D4 RID: 5844 RVA: 0x00072148 File Offset: 0x00070348
    private void AddKeyStone(float time) {
        int index = this.FindKeyStoneInsertIndex(time);
        ReplayCameraController.KeyStone item;
        switch (this.mode) {
            case ReplayCameraController.CameraMode.Free:
                item = new ReplayCameraController.FreeCameraKeyStone(this.cameraTransform, time);
                break;
            case ReplayCameraController.CameraMode.Orbit:
                item = new ReplayCameraController.OrbitCameraKeyStone(this.orbitRadialCoord, time);
                break;
            case ReplayCameraController.CameraMode.Tripod:
                item = new ReplayCameraController.TripodCameraKeyStone(this.cameraTransform, time);
                break;
            default:
                return;
        }
        this.keyStones.Insert(index, item);
    }

    // Token: 0x060016D5 RID: 5845 RVA: 0x000721B0 File Offset: 0x000703B0
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

    // Token: 0x060016D6 RID: 5846 RVA: 0x00072244 File Offset: 0x00070444
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
    }

    // Token: 0x060016D7 RID: 5847 RVA: 0x0007230C File Offset: 0x0007050C
    public void OnExitReplayEditor() {
        base.enabled = false;
        PlayerController.Instance.cameraController._actualCam.SetParent(this.cameraParent);
        CameraMovementController cameraMovementController = UnityEngine.Object.FindObjectOfType<CameraMovementController>();
        if (cameraMovementController) {
            cameraMovementController.enabled = true;
        }
    }

    // Token: 0x060016D8 RID: 5848 RVA: 0x00072350 File Offset: 0x00070550
    private void InputFreePosition(bool useDpadYForY = true, bool useRightStickForY = true) {
        float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
        float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
        float num = 0f;
        if (useDpadYForY) {
            num += PlayerController.Instance.inputController.player.GetAxis("DPadY");
        }
        if (useRightStickForY) {
            num += PlayerController.Instance.inputController.player.GetAxis("RightStickY");
        }
        Vector3 point = new Vector3(axis, num, axis2);
        Vector3 a = Quaternion.Euler(0f, this.cameraTransform.eulerAngles.y, 0f) * point;
        this.cameraTransform.position += a * this.FreeMoveSpeed;
    }

    // Token: 0x060016D9 RID: 5849 RVA: 0x0007242C File Offset: 0x0007062C
    public ReplayCameraController.KeyStone FindNextKeyStone(float time, bool left) {
        if (this.keyStones.Count == 0) {
            return null;
        }
        if (left) {
            using (List<ReplayCameraController.KeyStone>.Enumerator enumerator = this.keyStones.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    ReplayCameraController.KeyStone keyStone = enumerator.Current;
                    if (keyStone.time < time) {
                        return keyStone;
                    }
                }
                goto IL_8D;
            }
        }
        foreach (ReplayCameraController.KeyStone keyStone2 in this.keyStones) {
            if (keyStone2.time > time) {
                return keyStone2;
            }
        }
        IL_8D:
        return null;
    }

    // Token: 0x040010DB RID: 4315
    public KeyCode kc_SetKeyStone;

    // Token: 0x040010DC RID: 4316
    public KeyCode kc_DeleteKeyStone;

    // Token: 0x040010DD RID: 4317
    private Transform cameraTransform;

    // Token: 0x040010DE RID: 4318
    public ReplayCameraController.CameraMode mode;

    // Token: 0x040010DF RID: 4319
    public Vector3 lookDirection;

    // Token: 0x040010E0 RID: 4320
    public ReplayCameraController.Vector3Radial orbitRadialCoord;

    // Token: 0x040010E1 RID: 4321
    public float RotateSpeed;

    // Token: 0x040010E2 RID: 4322
    public float FreeMoveSpeed;

    // Token: 0x040010E3 RID: 4323
    public float OrbitMoveSpeed;

    // Token: 0x040010E4 RID: 4324
    private bool dpadCentered = true;

    // Token: 0x040010E5 RID: 4325
    private float xDownTime;

    // Token: 0x040010E6 RID: 4326
    private float keyStoneDeleteTolerance = 0.1f;

    // Token: 0x040010E7 RID: 4327
    public List<ReplayCameraController.KeyStone> keyStones;

    // Token: 0x040010E8 RID: 4328
    private Transform cameraParent;

    // Token: 0x040010E9 RID: 4329
    public bool CamFollowKeyStones;

    // Token: 0x040010EA RID: 4330
    private ReplayManager manager;

    // Token: 0x0200021F RID: 543
    public enum CameraMode {
        // Token: 0x040010EC RID: 4332
        Free,
        // Token: 0x040010ED RID: 4333
        Orbit,
        // Token: 0x040010EE RID: 4334
        Tripod
    }

    // Token: 0x02000220 RID: 544
    public struct Vector3Radial {
        // Token: 0x060016DA RID: 5850 RVA: 0x000116A1 File Offset: 0x0000F8A1
        public Vector3Radial(float p, float t, float r) {
            this.phi = p;
            this.theta = t;
            this.radius = r;
        }

        // Token: 0x060016DB RID: 5851 RVA: 0x000116B8 File Offset: 0x0000F8B8
        public Vector3Radial(Vector3 source) {
            this.radius = source.magnitude;
            this.phi = Mathf.Atan2(source.x, source.z);
            this.theta = Mathf.Acos(source.y / source.magnitude);
        }

        // Token: 0x170005A0 RID: 1440
        // (get) Token: 0x060016DC RID: 5852 RVA: 0x000724E8 File Offset: 0x000706E8
        public Vector3 cartesianCoords {
            get {
                return new Vector3(this.radius * Mathf.Sin(this.theta) * Mathf.Sin(this.phi), this.radius * Mathf.Cos(this.theta), this.radius * Mathf.Sin(this.theta) * Mathf.Cos(this.phi));
            }
        }

        // Token: 0x060016DD RID: 5853 RVA: 0x000116F7 File Offset: 0x0000F8F7
        public static ReplayCameraController.Vector3Radial Lerp(ReplayCameraController.Vector3Radial l, ReplayCameraController.Vector3Radial r, float t) {
            return new ReplayCameraController.Vector3Radial(Mathf.LerpAngle(l.phi, r.phi, t), Mathf.LerpAngle(l.theta, r.theta, t), Mathf.Lerp(l.radius, r.radius, t));
        }

        // Token: 0x040010EF RID: 4335
        public float phi;

        // Token: 0x040010F0 RID: 4336
        public float theta;

        // Token: 0x040010F1 RID: 4337
        public float radius;
    }

    // Token: 0x02000221 RID: 545
    [Serializable]
    public class KeyStone {
        // Token: 0x060016DF RID: 5855 RVA: 0x00011734 File Offset: 0x0000F934
        public virtual void ApplyTo(Transform t) {
            t.position = this.position;
            t.rotation = this.rotation;
        }

        // Token: 0x040010F2 RID: 4338
        public float time;

        // Token: 0x040010F3 RID: 4339
        public Vector3 position;

        // Token: 0x040010F4 RID: 4340
        public Quaternion rotation;
    }

    // Token: 0x02000222 RID: 546
    [Serializable]
    public class FreeCameraKeyStone : ReplayCameraController.KeyStone {
        // Token: 0x060016E0 RID: 5856 RVA: 0x0001174E File Offset: 0x0000F94E
        public FreeCameraKeyStone(Transform cameraTransform, float t) {
            this.position = cameraTransform.position;
            this.rotation = cameraTransform.rotation;
            this.time = t;
        }

        // Token: 0x060016E1 RID: 5857 RVA: 0x00011775 File Offset: 0x0000F975
        public FreeCameraKeyStone(Vector3 p, Quaternion r, float t) {
            this.position = p;
            this.rotation = r;
            this.time = t;
        }

        // Token: 0x060016E2 RID: 5858 RVA: 0x00072548 File Offset: 0x00070748
        public static ReplayCameraController.FreeCameraKeyStone Lerp(ReplayCameraController.KeyStone a, ReplayCameraController.KeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new ReplayCameraController.FreeCameraKeyStone(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), time);
        }

        // Token: 0x060016E3 RID: 5859 RVA: 0x00011792 File Offset: 0x0000F992
        public FreeCameraKeyStone(ReplayCameraController.KeyStone ks) {
            this.position = ks.position;
            this.rotation = ks.rotation;
            this.time = ks.time;
        }
    }

    // Token: 0x02000223 RID: 547
    [Serializable]
    public class OrbitCameraKeyStone : ReplayCameraController.KeyStone {
        // Token: 0x060016E4 RID: 5860 RVA: 0x00072598 File Offset: 0x00070798
        public override void ApplyTo(Transform t) {
            t.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
        }

        // Token: 0x060016E5 RID: 5861 RVA: 0x000725EC File Offset: 0x000707EC
        public static ReplayCameraController.OrbitCameraKeyStone Lerp(ReplayCameraController.OrbitCameraKeyStone a, ReplayCameraController.OrbitCameraKeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new ReplayCameraController.OrbitCameraKeyStone(ReplayCameraController.Vector3Radial.Lerp(a.radialPos, b.radialPos, t), time);
        }

        // Token: 0x060016E6 RID: 5862 RVA: 0x00072628 File Offset: 0x00070828
        public OrbitCameraKeyStone(ReplayCameraController.Vector3Radial radialPos, float t) {
            this.radialPos = radialPos;
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x060016E7 RID: 5863 RVA: 0x000726A0 File Offset: 0x000708A0
        public OrbitCameraKeyStone(Vector3 v, float t) {
            this.radialPos = new ReplayCameraController.Vector3Radial(v);
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x040010F5 RID: 4341
        public ReplayCameraController.Vector3Radial radialPos;
    }

    // Token: 0x02000224 RID: 548
    [Serializable]
    public class TripodCameraKeyStone : ReplayCameraController.KeyStone {
        // Token: 0x060016E8 RID: 5864 RVA: 0x000117BE File Offset: 0x0000F9BE
        public override void ApplyTo(Transform t) {
            t.position = this.position;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
        }

        // Token: 0x060016E9 RID: 5865 RVA: 0x00072720 File Offset: 0x00070920
        public TripodCameraKeyStone(Transform cameraTransform, float t) {
            this.position = cameraTransform.position;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x060016EA RID: 5866 RVA: 0x00072778 File Offset: 0x00070978
        public TripodCameraKeyStone(Vector3 p, float t) {
            this.position = p;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x060016EB RID: 5867 RVA: 0x000727C8 File Offset: 0x000709C8
        public static ReplayCameraController.TripodCameraKeyStone Lerp(ReplayCameraController.KeyStone a, ReplayCameraController.KeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new ReplayCameraController.TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, t), time);
        }
    }
}
}