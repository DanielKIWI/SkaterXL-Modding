using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200021E RID: 542
public class ReplayCameraController : MonoBehaviour
{
    // Token: 0x060016C2 RID: 5826
    public void Awake()
    {
        this.manager = base.GetComponent<ReplayManager>();
        this.cameraTransform = PlayerController.Instance.cameraController._actualCam;
        this.FreeMoveSpeed = 0.2f;
        this.OrbitMoveSpeed = 0.2f;
        this.RotateSpeed = 1.2f;
        this.mode = ReplayCameraController.CameraMode.Orbit;
        this.keyStones = new List<ReplayCameraController.KeyStone>();
        this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
    }

    // Token: 0x060016C4 RID: 5828
    public void Update()
    {
        this.DebugUpdate();
        this.InputModeChange();
        this.InputKeyStoneControll();
        switch (this.mode)
        {
            case ReplayCameraController.CameraMode.Free:
                this.InputFreePosition();
                this.InputFreeRotation();
                return;
            case ReplayCameraController.CameraMode.Orbit:
                this.InputOrbitMode();
                this.cameraTransform.position = PlayerController.Instance.skaterController.skaterTransform.position + this.orbitRadialCoord.cartesianCoords;
                this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
                return;
            case ReplayCameraController.CameraMode.Tripod:
                this.InputFreePosition();
                this.cameraTransform.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
                return;
            default:
                return;
        }
    }

    // Token: 0x060016C5 RID: 5829
    private void InputOrbitMode()
    {
        float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
        float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
        PlayerController.Instance.inputController.player.GetAxis("RightStickX");
        float axis3 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
        this.orbitRadialCoord.phi = Mathf.Repeat(this.orbitRadialCoord.phi + axis * this.OrbitMoveSpeed / this.orbitRadialCoord.radius, 6.28318548f);
        this.orbitRadialCoord.theta = Mathf.Clamp(this.orbitRadialCoord.theta - axis2 * this.OrbitMoveSpeed / this.orbitRadialCoord.radius, 0f, 3.14159274f);
        this.orbitRadialCoord.radius = Mathf.Clamp(this.orbitRadialCoord.radius - axis3 * this.FreeMoveSpeed, 0.1f, 100f);
    }

    // Token: 0x060016C6 RID: 5830
    public void OnGUI()
    {
        if (this.manager.guiHidden)
        {
            return;
        }
        float num = 200f;
        float x = (float)Screen.width - num - 20f;
        float num2 = (float)Screen.height / 2f - 100f;
        float num3 = 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "CamMode: " + Enum.GetName(typeof(ReplayCameraController.CameraMode), this.mode));
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "Y: Change Mode");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "X: Add KeyStone");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "Hold X: Delete KeyStone");
        num3 += 20f;
        GUI.Label(new Rect(x, num2 + num3, num, 20f), "DPadX: Jump to next KeyStone");
        num3 += 20f;
        switch (this.mode)
        {
            case ReplayCameraController.CameraMode.Free:
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStick: Move(xz)");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "DpadY: Move(y)");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "RightStick: Rotate");
                num3 += 20f;
                break;
            case ReplayCameraController.CameraMode.Orbit:
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStick: Orbit around Skater");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "RightStickY: Change Orbit Radius");
                num3 += 20f;
                break;
            case ReplayCameraController.CameraMode.Tripod:
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "LeftStick: Move(xz)");
                num3 += 20f;
                GUI.Label(new Rect(x, num2 + num3, num, 20f), "DpadY: Move(y)");
                num3 += 20f;
                break;
        }
        GUI.Box(new Rect(x, num2, num, num3), "Camera");
    }

    // Token: 0x060016C7 RID: 5831
    private void InputFreePosition()
    {
        float axis = PlayerController.Instance.inputController.player.GetAxis("LeftStickX");
        float axis2 = PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
        float y = PlayerController.Instance.inputController.player.GetAxis("DPadY") + PlayerController.Instance.inputController.player.GetAxis("LeftStickY");
        Vector3 point = new Vector3(axis, y, axis2);
        Vector3 a = Quaternion.Euler(this.cameraTransform.eulerAngles.x, 0f, this.cameraTransform.eulerAngles.z) * point;
        this.cameraTransform.position += a * this.FreeMoveSpeed;
    }

    // Token: 0x060016C8 RID: 5832
    private void InputFreeRotation()
    {
        float axis = PlayerController.Instance.inputController.player.GetAxis("RightStickX");
        float axis2 = PlayerController.Instance.inputController.player.GetAxis("RightStickY");
        Vector3 eulerAngles = this.cameraTransform.rotation.eulerAngles;
        eulerAngles.x -= axis2 * this.RotateSpeed;
        eulerAngles.y = Mathf.Clamp(eulerAngles.y + axis * this.RotateSpeed, -89f, 89f);
        eulerAngles.z = 0f;
        this.cameraTransform.rotation = Quaternion.Euler(eulerAngles);
    }

    // Token: 0x060016C9 RID: 5833
    private void SwitchModeTo(ReplayCameraController.CameraMode newValue)
    {
        ReplayCameraController.CameraMode cameraMode = this.mode;
        if (newValue == cameraMode)
        {
            return;
        }
        this.mode = newValue;
        if (newValue == ReplayCameraController.CameraMode.Orbit)
        {
            this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.transform.position);
        }
    }

    // Token: 0x060016CA RID: 5834
    private void InputModeChange()
    {
        if (PlayerController.Instance.inputController.player.GetButtonDown("Y"))
        {
            this.mode = ((this.mode >= ReplayCameraController.CameraMode.Tripod) ? ReplayCameraController.CameraMode.Free : (this.mode + 1));
        }
    }

    // Token: 0x060016CB RID: 5835
    private void InputKeyStoneControll()
    {
        float axis = PlayerController.Instance.inputController.player.GetAxis("DPadX");
        if ((double)Mathf.Abs(axis) > 0.10000000149011612)
        {
            if (this.dpadCentered)
            {
                ReplayCameraController.KeyStone keyStone = null;
                foreach (ReplayCameraController.KeyStone keyStone2 in this.keyStones)
                {
                    if (((axis > 0f && keyStone2.time > this.manager.playbackTime) || (axis < 0f && keyStone2.time < this.manager.playbackTime)) && (keyStone == null || Mathf.Abs(keyStone2.time - this.manager.playbackTime) < Mathf.Abs(keyStone.time - this.manager.playbackTime)))
                    {
                        keyStone = keyStone2;
                    }
                }
                if (keyStone != null)
                {
                    this.manager.SetPlaybackTime(keyStone.time);
                }
            }
            this.dpadCentered = false;
        }
        else
        {
            this.dpadCentered = true;
        }
        if (PlayerController.Instance.inputController.player.GetButtonDown("X"))
        {
            this.xDownTime = Time.unscaledTime;
        }
        if (PlayerController.Instance.inputController.player.GetButtonUp("X"))
        {
            if (Time.unscaledTime - this.xDownTime > 1f)
            {
                this.DeleteKeyStone();
                return;
            }
            this.AddKeyStone(this.manager.playbackTime);
        }
    }

    // Token: 0x060016CC RID: 5836
    private void DeleteKeyStone()
    {
        int index;
        if (!this.FindKeyStoneDeleteIndex(out index))
        {
            return;
        }
        this.keyStones.RemoveAt(index);
    }

    // Token: 0x060016CD RID: 5837
    private bool FindKeyStoneDeleteIndex(out int index)
    {
        for (int i = 0; i < this.keyStones.Count; i++)
        {
            if (Mathf.Abs(this.manager.playbackTime - this.keyStones[i].time) < this.keyStoneDeleteTolerance)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    // Token: 0x060016CE RID: 5838
    private void LerpKeyStones()
    {
        if (this.keyStones.Count <= 0)
        {
            return;
        }
        if (this.keyStones.Count == 1)
        {
            this.keyStones[0].ApplyTo(this.cameraTransform);
        }
        int num = this.FindLeftKeyStoneIndex();
        ReplayCameraController.KeyStone keyStone = this.keyStones[num];
        ReplayCameraController.KeyStone keyStone2 = this.keyStones[num + 1];
        if (keyStone is ReplayCameraController.FreeCameraKeyStone || keyStone2 is ReplayCameraController.FreeCameraKeyStone)
        {
            ReplayCameraController.FreeCameraKeyStone.Lerp(keyStone, keyStone2, this.manager.playbackTime).ApplyTo(this.cameraTransform);
        }
        if (keyStone is ReplayCameraController.TripodCameraKeyStone || keyStone2 is ReplayCameraController.TripodCameraKeyStone)
        {
            ReplayCameraController.TripodCameraKeyStone.Lerp(keyStone, keyStone2, this.manager.playbackTime).ApplyTo(this.cameraTransform);
        }
        if (keyStone is ReplayCameraController.OrbitCameraKeyStone && keyStone2 is ReplayCameraController.OrbitCameraKeyStone)
        {
            ReplayCameraController.OrbitCameraKeyStone.Lerp(keyStone as ReplayCameraController.OrbitCameraKeyStone, keyStone2 as ReplayCameraController.OrbitCameraKeyStone, this.manager.playbackTime).ApplyTo(this.cameraTransform);
        }
    }

    // Token: 0x060016CF RID: 5839
    private void DebugUpdate()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Transform transform = this.cameraTransform;
            this.LogHierarchy(base.transform);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Transform skaterTransform = PlayerController.Instance.skaterController.skaterTransform;
            this.LogHierarchy(skaterTransform);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            foreach (Transform t in this.manager.recorder.transformsToBeRecorded)
            {
                this.LogHierarchy(t);
            }
        }
    }

    // Token: 0x060016D0 RID: 5840
    private int FindLeftKeyStoneIndex()
    {
        if (this.manager.playbackTime < this.keyStones[0].time)
        {
            return 0;
        }
        for (int i = 0; i < this.keyStones.Count - 1; i++)
        {
            if (this.manager.playbackTime > this.keyStones[i].time && this.manager.playbackTime < this.keyStones[i + 1].time)
            {
                return i;
            }
        }
        return this.keyStones.Count - 2;
    }

    // Token: 0x060016D1 RID: 5841
    private void AddKeyStone(float time)
    {
        int index = this.FindKeyStoneInsertIndex(time);
        ReplayCameraController.KeyStone item;
        switch (this.mode)
        {
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

    // Token: 0x060016D2 RID: 5842
    private int FindKeyStoneInsertIndex(float time)
    {
        if (this.keyStones.Count == 0)
        {
            return 0;
        }
        if (time < this.keyStones[0].time)
        {
            return 0;
        }
        if (this.keyStones.Count == 1)
        {
            return 1;
        }
        for (int i = 0; i < this.keyStones.Count - 1; i++)
        {
            if (time > this.keyStones[i].time && time < this.keyStones[i + 1].time)
            {
                return i + 1;
            }
        }
        return this.keyStones.Count;
    }

    // Token: 0x060016D3 RID: 5843
    public void LogHierarchy(Transform t)
    {
        string text = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            text = text + " - " + t.name;
        }
        GUIConsole.Log(text);
    }

    // Token: 0x060016D4 RID: 5844
    public void OnStartReplayEditor()
    {
        base.enabled = true;
        if (this.mode == ReplayCameraController.CameraMode.Orbit)
        {
            this.orbitRadialCoord = new ReplayCameraController.Vector3Radial(this.cameraTransform.position - PlayerController.Instance.skaterController.skaterTransform.position);
        }
        this.cameraParent = this.cameraTransform.parent;
        PlayerController.Instance.cameraController._actualCam.SetParent(null);
    }

    // Token: 0x060016D5 RID: 5845
    public void OnExitReplayEditor()
    {
        base.enabled = false;
        PlayerController.Instance.cameraController._actualCam.SetParent(this.cameraParent);
    }

    // Token: 0x040010D4 RID: 4308
    public KeyCode kc_SetKeyStone;

    // Token: 0x040010D5 RID: 4309
    public KeyCode kc_DeleteKeyStone;

    // Token: 0x040010D6 RID: 4310
    private Transform cameraTransform;

    // Token: 0x040010D7 RID: 4311
    public ReplayCameraController.CameraMode mode;

    // Token: 0x040010D8 RID: 4312
    public Vector3 lookDirection;

    // Token: 0x040010D9 RID: 4313
    public ReplayCameraController.Vector3Radial orbitRadialCoord;

    // Token: 0x040010DA RID: 4314
    public float RotateSpeed;

    // Token: 0x040010DB RID: 4315
    public float FreeMoveSpeed;

    // Token: 0x040010DC RID: 4316
    public float OrbitMoveSpeed;

    // Token: 0x040010DD RID: 4317
    private ReplayManager manager;

    // Token: 0x040010DE RID: 4318
    private bool dpadCentered = true;

    // Token: 0x040010DF RID: 4319
    private float xDownTime;

    // Token: 0x040010E0 RID: 4320
    private float keyStoneDeleteTolerance = 0.1f;

    // Token: 0x040010E1 RID: 4321
    public List<ReplayCameraController.KeyStone> keyStones;

    // Token: 0x040010E2 RID: 4322
    private Transform cameraParent;

    // Token: 0x0200021F RID: 543
    public enum CameraMode
    {
        // Token: 0x040010E4 RID: 4324
        Free,
        // Token: 0x040010E5 RID: 4325
        Orbit,
        // Token: 0x040010E6 RID: 4326
        Tripod
    }

    // Token: 0x02000220 RID: 544
    public struct Vector3Radial
    {
        // Token: 0x060016D6 RID: 5846
        public Vector3Radial(float p, float t, float r)
        {
            this.phi = p;
            this.theta = t;
            this.radius = r;
        }

        // Token: 0x060016D7 RID: 5847
        public Vector3Radial(Vector3 source)
        {
            this.radius = source.magnitude;
            this.phi = Mathf.Atan2(source.y, source.x);
            this.theta = Mathf.Acos(source.z / source.magnitude);
        }

        // Token: 0x170005A0 RID: 1440
        // (get) Token: 0x060016D8 RID: 5848
        public Vector3 cartesianCoords
        {
            get
            {
                return new Vector3(this.radius * Mathf.Sin(this.theta) * Mathf.Sin(this.phi), this.radius * Mathf.Cos(this.theta), this.radius * Mathf.Sin(this.theta) * Mathf.Cos(this.phi));
            }
        }

        // Token: 0x060016D9 RID: 5849
        public static ReplayCameraController.Vector3Radial Lerp(ReplayCameraController.Vector3Radial l, ReplayCameraController.Vector3Radial r, float t)
        {
            return new ReplayCameraController.Vector3Radial(Mathf.LerpAngle(l.phi, r.phi, t), Mathf.LerpAngle(l.theta, r.theta, t), Mathf.Lerp(l.radius, r.radius, t));
        }

        // Token: 0x040010E7 RID: 4327
        public float phi;

        // Token: 0x040010E8 RID: 4328
        public float theta;

        // Token: 0x040010E9 RID: 4329
        public float radius;
    }

    // Token: 0x02000221 RID: 545
    public class KeyStone
    {
        // Token: 0x060016DB RID: 5851
        public virtual void ApplyTo(Transform t)
        {
            t.position = this.position;
            t.rotation = this.rotation;
        }

        // Token: 0x040010EA RID: 4330
        public float time;

        // Token: 0x040010EB RID: 4331
        public Vector3 position;

        // Token: 0x040010EC RID: 4332
        public Quaternion rotation;
    }

    // Token: 0x02000222 RID: 546
    public class FreeCameraKeyStone : ReplayCameraController.KeyStone
    {
        // Token: 0x060016DC RID: 5852
        public FreeCameraKeyStone(Transform cameraTransform, float t)
        {
            this.position = cameraTransform.position;
            this.rotation = cameraTransform.rotation;
            this.time = t;
        }

        // Token: 0x060016DD RID: 5853
        public FreeCameraKeyStone(Vector3 p, Quaternion r, float t)
        {
            this.position = p;
            this.rotation = r;
            this.time = t;
        }

        // Token: 0x060016DE RID: 5854
        public static ReplayCameraController.FreeCameraKeyStone Lerp(ReplayCameraController.KeyStone a, ReplayCameraController.KeyStone b, float time)
        {
            float t = (time - a.time) / (b.time - a.time);
            return new ReplayCameraController.FreeCameraKeyStone(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), time);
        }

        // Token: 0x060016DF RID: 5855
        public FreeCameraKeyStone(ReplayCameraController.KeyStone ks)
        {
            this.position = ks.position;
            this.rotation = ks.rotation;
            this.time = ks.time;
        }
    }

    // Token: 0x02000223 RID: 547
    public class OrbitCameraKeyStone : ReplayCameraController.KeyStone
    {
        // Token: 0x060016E0 RID: 5856
        public override void ApplyTo(Transform t)
        {
            t.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
        }

        // Token: 0x060016E1 RID: 5857
        public static ReplayCameraController.OrbitCameraKeyStone Lerp(ReplayCameraController.OrbitCameraKeyStone a, ReplayCameraController.OrbitCameraKeyStone b, float time)
        {
            float t = (time - a.time) / (b.time - a.time);
            return new ReplayCameraController.OrbitCameraKeyStone(ReplayCameraController.Vector3Radial.Lerp(a.radialPos, b.radialPos, t), time);
        }

        // Token: 0x060016E2 RID: 5858
        public OrbitCameraKeyStone(ReplayCameraController.Vector3Radial radialPos, float t)
        {
            this.radialPos = radialPos;
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x060016E3 RID: 5859
        public OrbitCameraKeyStone(Vector3 v, float t)
        {
            this.radialPos = new ReplayCameraController.Vector3Radial(v);
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x040010ED RID: 4333
        public ReplayCameraController.Vector3Radial radialPos;
    }

    // Token: 0x02000224 RID: 548
    public class TripodCameraKeyStone : ReplayCameraController.KeyStone
    {
        // Token: 0x060016E4 RID: 5860
        public override void ApplyTo(Transform t)
        {
            t.position = this.position;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
        }

        // Token: 0x060016E5 RID: 5861
        public TripodCameraKeyStone(Transform cameraTransform, float t)
        {
            this.position = cameraTransform.position;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x060016E6 RID: 5862
        public TripodCameraKeyStone(Vector3 p, float t)
        {
            this.position = p;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
        }

        // Token: 0x060016E7 RID: 5863
        public static ReplayCameraController.TripodCameraKeyStone Lerp(ReplayCameraController.KeyStone a, ReplayCameraController.KeyStone b, float time)
        {
            float t = (time - a.time) / (b.time - a.time);
            return new ReplayCameraController.TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, t), time);
        }
    }
}
