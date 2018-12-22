//Changed Function:

// Token: 0x0600017A RID: 378
public void DoBail() {
    if (SkateTrainer.CoachFrank.respawnOnMarker) {
        this.respawn.DoRespawn();
        return;
    }
    this.respawn.DoTmpRespawn();
}


//Added Attribute:

private float spinVelocity;
private bool spinVelocityEnabled;

//Added Code: 
public void Start() {
    //..
    spinVelocityEnabled = false;
    //..
}
//=====

public float getSpinVelocity() {
    return this.spinVelocity;
}

// Token: 0x0600447D RID: 17533
public void resetSpinVelocity() {
    this.spinVelocity = 0f;
}

// Token: 0x06004882 RID: 18562
public void addSpinVelocity(string side) {
    if (side == "right") {
        this.spinVelocity += 0.25f;
        return;
    }
    this.spinVelocity -= 0.25f;
}


//edited code:

public void TurnLeft(float p_value, InputController.TurningMode p_turningMode) {
    //...
    switch (p_turningMode) {
        //...
        case InputController.TurningMode.InAir:
            if (this.spinVelocityEnabled) {
                this.skaterController.AddTurnTorque(-p_value + this.getSpinVelocity());
                this.addSpinVelocity("left");
                return;
            }
            this.skaterController.AddTurnTorque(-p_value);
    }
    return;

    //...
}
// Token: 0x06000193 RID: 403
public void TurnRight(float p_value, InputController.TurningMode p_turningMode) {
    //...
    switch (p_turningMode) {
        //...
        case InputController.TurningMode.InAir:
            if (this.spinVelocityEnabled) {
                this.skaterController.AddTurnTorque(p_value + this.getSpinVelocity());
                this.addSpinVelocity("right");
                return;
            }
            this.skaterController.AddTurnTorque(p_value);
            return;

            //...
    }
    //...
}


public void AnimSetManual(bool p_value, float p_manualAxis)
	//...
	this.resetSpinVelocity();
    this.Manualling = p_value;
    //...
}

// Token: 0x060001BA RID: 442
public void AnimSetNoseManual(bool p_value, float p_manualAxis) {
    //...

    this.resetSpinVelocity();

    this.Manualling = p_value;

}


public bool IsRespawning {
    get {
        this.resetSpinVelocity();
        return this._isRespawning;
    }
    set {
        this.resetSpinVelocity();
        this._isRespawning = value;
    }
}
