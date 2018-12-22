using System;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using UnityEngine;

// Token: 0x0200002C RID: 44
public class Respawn : MonoBehaviour {

    // Token: 0x0600023D RID: 573 RVA: 0x0001E32C File Offset: 0x0001C52C
    public void Update() {
        //...
        //Added lines:
        if (!this.respawning && !this.puppetMaster.isBlending && Time.time - this.lastTmpSave > 0.5f && PlayerController.Instance.IsGrounded() && !this.bail.bailed && Time.timeScale != 0f) {
            this.lastTmpSave = Time.time;
            this.SetTmpSpawnPos();
        }
        //...
    }
    
    ///////////////////////////  Added Functions

    // Token: 0x06000244 RID: 580 RVA: 0x0001EB60 File Offset: 0x0001CD60
    public void DoTmpRespawn() {
        if (this._canPress && !this.respawning) {
            PlayerController.Instance.IsRespawning = true;
            this.respawning = true;
            this._canPress = false;
            this.GetTmpSpawnPos();
            PlayerController.Instance.CancelInvoke("DoBail");
            base.CancelInvoke("DelayPress");
            base.CancelInvoke("EndRespawning");
            base.Invoke("DelayPress", 0.4f);
            base.Invoke("EndRespawning", 0.25f);
        }
    }

    // Token: 0x06000245 RID: 581 RVA: 0x0001EBE4 File Offset: 0x0001CDE4
    private void GetTmpSpawnPos() {
        base.CancelInvoke("DoRespawn");
        PlayerController.Instance.CancelRespawnInvoke();
        this.puppetMaster.FixTargetToSampledState(1f);
        this.puppetMaster.FixMusclePositions();
        this._behaviourPuppet.StopAllCoroutines();
        this._finalIk.enabled = false;
        for (int i = 0; i < this.getSpawn.Length; i++) {
            this.getSpawn[i].position = this._setTmpPos[i];
            this.getSpawn[i].rotation = this._setTmpRot[i];
        }
        this.bail.bailed = false;
        PlayerController.Instance.playerSM.OnRespawnSM();
        PlayerController.Instance.ResetIKOffsets();
        PlayerController.Instance.cameraController._leanForward = false;
        PlayerController.Instance.cameraController._pivot.rotation = PlayerController.Instance.cameraController._pivotCentered.rotation;
        PlayerController.Instance.comController.COMRigidbody.velocity = Vector3.zero;
        PlayerController.Instance.boardController.boardRigidbody.velocity = Vector3.zero;
        PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
        PlayerController.Instance.boardController.frontTruckRigidbody.velocity = Vector3.zero;
        PlayerController.Instance.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
        PlayerController.Instance.boardController.backTruckRigidbody.velocity = Vector3.zero;
        PlayerController.Instance.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
        PlayerController.Instance.skaterController.skaterRigidbody.velocity = Vector3.zero;
        PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
        PlayerController.Instance.skaterController.skaterRigidbody.useGravity = false;
        PlayerController.Instance.boardController.IsBoardBackwards = this._backwards;
        PlayerController.Instance.SetBoardToMaster();
        PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
        PlayerController.Instance.ResetAllAnimations();
        PlayerController.Instance.animationController.ForceAnimation("Riding");
        PlayerController.Instance.boardController.firstVel = 0f;
        PlayerController.Instance.boardController.secondVel = 0f;
        PlayerController.Instance.boardController.thirdVel = 0f;
        PlayerController.Instance.skaterController.ResetRotationLerps();
        PlayerController.Instance.SetLeftIKLerpTarget(0f);
        PlayerController.Instance.SetRightIKLerpTarget(0f);
        PlayerController.Instance.SetMaxSteeze(0f);
        PlayerController.Instance.AnimSetPush(false);
        PlayerController.Instance.CrossFadeAnimation("Riding", 0.05f);
        PlayerController.Instance.cameraController.ResetAllCamera();
        this.puppetMaster.targetRoot.position = this._setTmpPos[1] + this._playerOffset;
        this.puppetMaster.targetRoot.rotation = this._setTmpRot[0];
        this.puppetMaster.angularLimits = false;
        this.puppetMaster.Resurrect();
        this.puppetMaster.state = PuppetMaster.State.Alive;
        this.puppetMaster.targetAnimator.Play(this._idleAnimation, 0, 0f);
        this._behaviourPuppet.SetState(BehaviourPuppet.State.Puppet);
        this.puppetMaster.Teleport(this._setTmpPos[1] + this._playerOffset, this._setTmpRot[0], true);
        PlayerController.Instance.SetIKOnOff(1f);
        PlayerController.Instance.skaterController.skaterRigidbody.useGravity = false;
        PlayerController.Instance.skaterController.skaterRigidbody.constraints = RigidbodyConstraints.None;
        this._finalIk.enabled = true;
        this._retryRespawn = false;
    }

    // Token: 0x06000246 RID: 582 RVA: 0x0001EFC8 File Offset: 0x0001D1C8
    private void SetTmpSpawnPos() {
        Quaternion quaternion = Quaternion.LookRotation(this.getSpawn[0].rotation * Vector3.forward, Vector3.up);
        this._backwards = PlayerController.Instance.GetBoardBackwards();
        for (int i = 0; i < this._setTmpPos.Length; i++) {
            if ((float)i == 0f) {
                this._setTmpPos[i] = this.getSpawn[1].position + this._playerOffset;
                this._setTmpRot[i] = quaternion;
            } else if (i == 5) {
                this._setTmpPos[i] = this.getSpawn[i].position;
                this._setTmpRot[i] = quaternion;
            } else if (i == 7) {
                this._setTmpPos[i] = this.getSpawn[1].position + this._playerOffset;
                this._setTmpRot[i] = quaternion;
            } else {
                this._setTmpPos[i] = this.getSpawn[i].position;
                this._setTmpRot[i] = this.getSpawn[i].rotation;
            }
        }
    }

    ////////////////////////  Added Variables

    // Token: 0x04000253 RID: 595
    private Vector3[] _setTmpPos = new Vector3[8];

    // Token: 0x04000254 RID: 596
    private Quaternion[] _setTmpRot = new Quaternion[8];

    // Token: 0x04000255 RID: 597
    private float lastTmpSave;
}
