using System;
using UnityEngine;

// Token: 0x0200021A RID: 538
public class SkateTrainer : MonoBehaviour {
    // Token: 0x060016AE RID: 5806
    public void Start() {
        this.showMenu = false;
        this.colorHax = default(Color);
        ColorUtility.TryParseHtmlString("#00FF00", out this.colorHax);
        this.fontLarge = new GUIStyle();
        this.fontMed = new GUIStyle();
        this.fontSmall = new GUIStyle();
        this.fontLarge.fontSize = 32;
        this.fontLarge.normal.textColor = Color.white;
        this.fontMed.fontSize = 14;
        this.fontMed.normal.textColor = this.colorHax;
        this.fontSmall.fontSize = 12;
        this.fontSmall.normal.textColor = this.colorHax;
        PlayerController.Instance.popForce = 3f;
        this.autoSlowmo = false;
        this.baseTimeScale = 1f;
        this.sloMoTimeFactor = 0.6f;
    }

    // Token: 0x060016B0 RID: 5808
    public void Update() {
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        if (Input.GetKey(KeyCode.F8) && (double)(realtimeSinceStartup - this.btnLastPressed) > 0.15) {
            this.btnLastPressed = realtimeSinceStartup;
            this.showMenu = !this.showMenu;
        }
        PlayerController.Instance.popForce = this.InputFloatChange("PopForce", KeyCode.P, PlayerController.Instance.popForce, 0.2f, 0f, 8f, 3f, 0.15f);
        this.sloMoTimeFactor = this.InputFloatChange("SloMoFactor", KeyCode.T, this.sloMoTimeFactor, 0.1f, 0f, 1f, 0.6f, 0.15f);
        this.baseTimeScale = this.InputFloatChange("BaseTimeScale", KeyCode.T, this.baseTimeScale, 0.1f, 0f, 2f, 0.6f, 0.15f);
        this.respawnOnMarker = this.InputBoolChange("RespawnOnMarker", KeyCode.M, this.respawnOnMarker, 0.2f);
        this.autoSlowmo = this.InputBoolChange("Auto SloMo", KeyCode.A, this.autoSlowmo, 0.2f);
        PlayerController.Instance.spinVelocityEnabled = this.InputBoolChange("Auto SloMo", KeyCode.L, PlayerController.Instance.spinVelocityEnabled, 0.2f);
        this.disablePushPowerDecrement = this.InputBoolChange("DisablePushReduction", KeyCode.R, this.disablePushPowerDecrement, 0.2f);
        PlayerController.Instance.skaterController.pushForce = this.InputFloatChange("PushForce", KeyCode.F, PlayerController.Instance.skaterController.pushForce, 0.2f, 0f, 20f, 6f, 0.15f);
        this.ControllTime();
    }

    // Token: 0x060016B1 RID: 5809
    private void showMessage(string msg) {
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        this.tmpMessage = new SkateTrainer.TmpMessage {
            msg = msg,
            epoch = realtimeSinceStartup
        };
    }

    // Token: 0x060016B2 RID: 5810
    private void OnGUI() {
        if (!this.showMenu) {
            return;
        }
        GUI.color = this.colorHax;
        float num5 = 20f;
        float num2 = 20f;
        float num3 = 300f;
        float width = 250f;
        float x = num5 + 10f;
        GUI.Box(new Rect(num5, num2, width, num3), "XLShredMenu");
        float num4 = 30f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "Hold shift to decrease value", this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "LB, RB - Slowmo", this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "M - RespawnOnMarker " + (this.respawnOnMarker ? "on" : "off"), this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "A - Auto Slowmo " + (this.autoSlowmo ? "on" : "off"), this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "S - SlowmoFactor: " + this.sloMoTimeFactor.ToString("0.#"), this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "T - BaseTimeFactor: " + this.baseTimeScale.ToString("0.#"), this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "P - PopForce: " + PlayerController.Instance.popForce.ToString("0.#") + "(3.0)", this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "R - Disable Push Reduction " + (this.disablePushPowerDecrement ? "on" : "off"), this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "F - PushForce: " + PlayerController.Instance.skaterController.pushForce.ToString("0.#") + "(6.0)", this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num2 + num4, 200f, 40f), "L - Faster body spin: " + (PlayerController.Instance.spinVelocityEnabled ? "on" : "off"), this.fontSmall);
        num4 += 20f;
        GUI.Label(new Rect(x, num3 - 20f, width, 40f), "v0.2 by dsc, RafahelBF, Kiwi", this.fontSmall);
        GUI.Label(new Rect(x, num3 - 0f, width, 40f), "Commander klepto", this.fontSmall);
        if (this.tmpMessage != null) {
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            GUI.color = Color.white;
            GUI.Label(new Rect(20f, (float)(Screen.height - 50), 600f, 100f), this.tmpMessage.msg, this.fontLarge);
            if (realtimeSinceStartup - this.tmpMessage.epoch > 1f) {
                this.tmpMessage = null;
            }
        }
    }

    // Token: 0x060016B3 RID: 5811
    private void ControllTime() {
        if (this.autoSlowmo) {
            if (!PlayerController.Instance.boardController.AllDown) {
                this.timeScaleTarget = this.sloMoTimeFactor;
            } else {
                this.timeScaleTarget = this.baseTimeScale;
            }
        } else {
            this.timeScaleTarget = this.baseTimeScale;
        }
        if (PlayerController.Instance.inputController.player.GetButton("LB")) {
            this.timeScaleTarget *= this.sloMoTimeFactor;
        }
        if (PlayerController.Instance.inputController.player.GetButton("RB")) {
            this.timeScaleTarget *= this.sloMoTimeFactor;
        }
        if (Math.Round((double)Time.timeScale, 1) != (double)this.timeScaleTarget) {
            Time.timeScale += (this.timeScaleTarget - Time.timeScale) * Time.deltaTime * 15f;
            return;
        }
        Time.timeScale = this.timeScaleTarget;
    }

    // Token: 0x060016B4 RID: 5812
    public void Awake() {
        SkateTrainer.CoachFrank = this;
    }

    // Token: 0x060016B5 RID: 5813
    private float InputFloatChange(string description, KeyCode key, float value, float delta, float min, float max, float defaultValue = 1f, float deadTime = 0.15f) {
        if (Input.GetKey(key) && (double)(Time.realtimeSinceStartup - this.btnLastPressed) > (double)deadTime) {
            this.btnLastPressed = Time.realtimeSinceStartup;
            if (Input.GetKey(KeyCode.LeftShift)) {
                value -= delta;
            } else {
                value += delta;
            }
            value = Mathf.Clamp(value, min, max);
            this.showMessage(string.Concat(new object[]
            {
                description,
                ": ",
                string.Format("{0:0.0}", value),
                ", min: ",
                min.ToString("0.0"),
                ", max:",
                max.ToString("0.0"),
                ", default: ",
                defaultValue
            }));
        }
        return value;
    }

    // Token: 0x060016B6 RID: 5814
    private bool InputBoolChange(string description, KeyCode key, bool value, float deadTime = 0.2f) {
        if (Input.GetKey(key) && (double)(Time.realtimeSinceStartup - this.btnLastPressed) > (double)deadTime) {
            this.btnLastPressed = Time.realtimeSinceStartup;
            value = !value;
            this.showMessage(description + (value ? "ON" : "OFF"));
        }
        return value;
    }

    // Token: 0x040010C2 RID: 4290
    private string grindString;

    // Token: 0x040010C3 RID: 4291
    private float timeScaleTarget;

    // Token: 0x040010C4 RID: 4292
    private bool autoSlowmo;

    // Token: 0x040010C5 RID: 4293
    private SkateTrainer.TmpMessage tmpMessage;

    // Token: 0x040010C6 RID: 4294
    private float btnLastPressed;

    // Token: 0x040010C7 RID: 4295
    private bool showMenu;

    // Token: 0x040010C8 RID: 4296
    private Color colorHax;

    // Token: 0x040010C9 RID: 4297
    private GUIStyle fontLarge;

    // Token: 0x040010CA RID: 4298
    private GUIStyle fontMed;

    // Token: 0x040010CB RID: 4299
    private GUIStyle fontSmall;

    // Token: 0x040010CC RID: 4300
    private float sloMoTimeFactor = 0.5f;

    // Token: 0x040010CD RID: 4301
    private float baseTimeScale;

    // Token: 0x040010CE RID: 4302
    public static SkateTrainer CoachFrank;

    // Token: 0x040010CF RID: 4303
    public bool disablePushPowerDecrement;

    // Token: 0x040010D0 RID: 4304
    public bool respawnOnMarker;

    // Token: 0x0200021B RID: 539
    private class TmpMessage {
        // Token: 0x1700059E RID: 1438
        // (get) Token: 0x060016B7 RID: 5815
        // (set) Token: 0x060016B8 RID: 5816
        public string msg { get; set; }

        // Token: 0x1700059F RID: 1439
        // (get) Token: 0x060016B9 RID: 5817
        // (set) Token: 0x060016BA RID: 5818
        public float epoch { get; set; }
    }
}
