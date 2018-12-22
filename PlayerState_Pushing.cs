//Changed Functions:

//Explenation for pushPower :
//  on Push  = 0
//  on Update += Time.deltaTime
// Token: 0x060003EE RID: 1006
public override void OnPush() {
    if (SkateTrainer.CoachFrank.disablePushPowerDecrement) {
        this._pushPower = 1f;
    } else {    //Normal behaviour
        this._pushPower = Mathf.Clamp(this._pushPower, (this._pushCount > 2) ? 0.9f : 0.5f, 1f);
    }
    PlayerController.Instance.AddPushForce(PlayerController.Instance.GetPushForce() * 1.8f * this._pushPower);
    this._pushPower = 0f;
}