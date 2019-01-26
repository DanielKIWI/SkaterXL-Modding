using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Harmony12;
using Dreamteck.Splines;

namespace XLShredGrindToGrind.Patches {
    [HarmonyPatch(typeof(PlayerState_Grinding))]
    [HarmonyPatch("OnGrindEnded")]
    public static class PlayerState_Grinding_Patch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = instructions.ToList();
            var canGrindCode = codes[95]; //That line represents the value to that PlayerState_Air._canGrind gets set.
            //95 = 116 - 13 - 8,  Explanation: boolean value gets pushed to the stack at line 116 in OnGrindEnded IL code, function code starts at 13, 8 empty lines 
            canGrindCode.opcode = OpCodes.Ldc_I4_1;     //Setting bool for PlayerState_Air._canGrind 
            return codes.AsEnumerable();
        }
    }
}


// PlayerState_Grinding.OnGrindEnded as C# code

// Token: 0x06000396 RID: 918 RVA: 0x00026494 File Offset: 0x00024694
//public override void OnGrindEnded() {
//    PlayerController.Instance.boardController.boardRigidbody.velocity = this._lastVelocity;
//    PlayerController.Instance.boardController.backTruckRigidbody.velocity = this._lastVelocity;
//    PlayerController.Instance.boardController.frontTruckRigidbody.velocity = this._lastVelocity;
//    PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
//    PlayerController.Instance.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
//    PlayerController.Instance.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
//    PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
//    if (PlayerController.Instance.TwoWheelsDown()) {
//        PlayerController.Instance.AnimGrindTransition(false);
//        PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
//        PlayerController.Instance.SetBoardToMaster();
//        if (!PlayerController.Instance.IsRespawning) {
//            PlayerController.Instance.CrossFadeAnimation("Riding", 0.3f);
//        }
//        base.DoTransition(typeof(PlayerState_Riding), null);
//        return;
//    }
//    if (PlayerController.Instance.TimeTilLand() < 0.1f) {
//        PlayerController.Instance.AnimLandedEarly(true);
//    }
//    PlayerController.Instance.AnimGrindTransition(false);
//    PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
//    PlayerController.Instance.SetSkaterToMaster();
//    if (!PlayerController.Instance.IsRespawning) {
//        PlayerController.Instance.CrossFadeAnimation("Extend", 0.3f);
//    }
//    object[] args = new object[]
//    {
//        true,
//        false,                                                    //<-------   This bool represents canGrind
//        this._spline
//    };
//    base.DoTransition(typeof(PlayerState_InAir), args);
//}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// PlayerState_Grinding.OnGrindEnded as C# code

//// Token: 0x06000396 RID: 918 RVA: 0x00026494 File Offset: 0x00024694
//.method public hidebysig virtual
//    instance void OnGrindEnded() cil managed {
//	// Header Size: 12 bytes
//	// Code Size: 418 (0x1A2) bytes
//	// LocalVarSig Token: 0x1100003B RID: 59
//	.maxstack 3
//	.locals init (
//		[0] object[]
//	)

//	/* 0x000246A0 2853010006   */ IL_0000: call      class PlayerController PlayerController::get_Instance()
//    /* 0x000246A5 7BF5010004   */ IL_0005: ldfld class BoardController PlayerController::boardController
//    /* 0x000246AA 7B40000004   */ IL_000A: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody BoardController::boardRigidbody
//    /* 0x000246AF 02           */ IL_000F: ldarg.0
//    /* 0x000246B0 7BA7030004   */ IL_0010: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 PlayerState_Grinding::_lastVelocity
//    /* 0x000246B5 6F6F00000A   */ IL_0015: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_velocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x000246BA 2853010006   */ IL_001A: call class PlayerController PlayerController::get_Instance()
//    /* 0x000246BF 7BF5010004   */ IL_001F: ldfld class BoardController PlayerController::boardController
//    /* 0x000246C4 7B42000004   */ IL_0024: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody BoardController::backTruckRigidbody
//    /* 0x000246C9 02           */ IL_0029: ldarg.0
//    /* 0x000246CA 7BA7030004   */ IL_002A: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 PlayerState_Grinding::_lastVelocity
//    /* 0x000246CF 6F6F00000A   */ IL_002F: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_velocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x000246D4 2853010006   */
//                                  IL_0034: call class PlayerController PlayerController::get_Instance()
//    /* 0x000246D9 7BF5010004   */ IL_0039: ldfld class BoardController PlayerController::boardController
//    /* 0x000246DE 7B41000004   */ IL_003E: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody BoardController::frontTruckRigidbody
//    /* 0x000246E3 02           */ IL_0043: ldarg.0
//    /* 0x000246E4 7BA7030004   */ IL_0044: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 PlayerState_Grinding::_lastVelocity
//    /* 0x000246E9 6F6F00000A   */ IL_0049: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_velocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x000246EE 2853010006   */
//                                  IL_004E: call class PlayerController PlayerController::get_Instance()
//    /* 0x000246F3 7BF5010004   */ IL_0053: ldfld class BoardController PlayerController::boardController
//    /* 0x000246F8 7B40000004   */ IL_0058: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody BoardController::boardRigidbody
//    /* 0x000246FD 288200000A   */ IL_005D: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
//    /* 0x00024702 6F8300000A   */ IL_0062: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00024707 2853010006   */
//                                  IL_0067: call class PlayerController PlayerController::get_Instance()
//    /* 0x0002470C 7BF5010004   */ IL_006C: ldfld class BoardController PlayerController::boardController
//    /* 0x00024711 7B42000004   */ IL_0071: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody BoardController::backTruckRigidbody
//    /* 0x00024716 288200000A   */ IL_0076: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
//    /* 0x0002471B 6F8300000A   */ IL_007B: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00024720 2853010006   */
//                                  IL_0080: call class PlayerController PlayerController::get_Instance()
//    /* 0x00024725 7BF5010004   */ IL_0085: ldfld class BoardController PlayerController::boardController
//    /* 0x0002472A 7B41000004   */ IL_008A: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody BoardController::frontTruckRigidbody
//    /* 0x0002472F 288200000A   */ IL_008F: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
//    /* 0x00024734 6F8300000A   */ IL_0094: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00024739 2853010006   */
//                                  IL_0099: call class PlayerController PlayerController::get_Instance()
//    /* 0x0002473E 7BF4010004   */ IL_009E: ldfld class SkaterController PlayerController::skaterController
//    /* 0x00024743 7B60020004   */ IL_00A3: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody SkaterController::skaterRigidbody
//    /* 0x00024748 288200000A   */ IL_00A8: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
//    /* 0x0002474D 6F8300000A   */ IL_00AD: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00024752 2853010006   */
//                                  IL_00B2: call class PlayerController PlayerController::get_Instance()
//    /* 0x00024757 6FF6010006   */ IL_00B7: callvirt instance bool PlayerController::TwoWheelsDown()
//    /* 0x0002475C 2C53         */ IL_00BC: brfalse.s IL_0111

//    /* 0x0002475E 2853010006   */ IL_00BE: call class PlayerController PlayerController::get_Instance()
//    /* 0x00024763 16           */ IL_00C3: ldc.i4.0
//	  /* 0x00024764 6FC0010006   */ IL_00C4: callvirt instance void PlayerController::AnimGrindTransition(bool)
//    /* 0x00024769 2853010006   */ IL_00C9: call class PlayerController PlayerController::get_Instance()
//    /* 0x0002476E 16           */ IL_00CE: ldc.i4.0
//	  /* 0x0002476F 6F90010006   */ IL_00CF: callvirt instance void PlayerController::SetTurningMode(valuetype InputController/TurningMode)
//    /* 0x00024774 2853010006   */ IL_00D4: call class PlayerController PlayerController::get_Instance()
//    /* 0x00024779 6FCE010006   */ IL_00D9: callvirt instance void PlayerController::SetBoardToMaster()
//    /* 0x0002477E 2853010006   */ IL_00DE: call class PlayerController PlayerController::get_Instance()
//    /* 0x00024783 6F55010006   */ IL_00E3: callvirt instance bool PlayerController::get_IsRespawning()
//    /* 0x00024788 2D14         */ IL_00E8: brtrue.s IL_00FE

//    /* 0x0002478A 2853010006   */ IL_00EA: call class PlayerController PlayerController::get_Instance()
//    /* 0x0002478F 72ED010070   */ IL_00EF: ldstr     "Riding"
//	  /* 0x00024794 229A99993E   */ IL_00F4: ldc.r4    0.3
//	  /* 0x00024799 6F96010006   */ IL_00F9: callvirt instance void PlayerController::CrossFadeAnimation(string, float32)

//    /* 0x0002479E 02           */ IL_00FE: ldarg.0
//	  /* 0x0002479F D054000002   */ IL_00FF: ldtoken PlayerState_Riding
//    /* 0x000247A4 289F01000A   */ IL_0104: call class [mscorlib]System.Type[mscorlib] System.Type::GetTypeFromHandle(valuetype[mscorlib] System.RuntimeTypeHandle)
//    /* 0x000247A9 14           */ IL_0109: ldnull
//    /* 0x000247AA 2845110006   */ IL_010A: call instance bool FSMHelper.BaseFSMState::DoTransition(class [mscorlib] System.Type, object[])
//	  /* 0x000247AF 26           */ IL_010F: pop
//    /* 0x000247B0 2A           */ IL_0110: ret

//    /* 0x000247B1 2853010006   */ IL_0111: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247B6 6F0F020006   */ IL_0116: callvirt instance float32 PlayerController::TimeTilLand()
//    /* 0x000247BB 22CDCCCC3D   */ IL_011B: ldc.r4    0.1
//	  /* 0x000247C0 340B         */ IL_0120: bge.un.s IL_012D

//    /* 0x000247C2 2853010006   */ IL_0122: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247C7 17           */ IL_0127: ldc.i4.1
//	  /* 0x000247C8 6FB5010006   */ IL_0128: callvirt instance void PlayerController::AnimLandedEarly(bool)

//    /* 0x000247CD 2853010006   */ IL_012D: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247D2 16           */ IL_0132: ldc.i4.0
//	  /* 0x000247D3 6FC0010006   */ IL_0133: callvirt instance void PlayerController::AnimGrindTransition(bool)
//    /* 0x000247D8 2853010006   */ IL_0138: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247DD 18           */ IL_013D: ldc.i4.2
//	  /* 0x000247DE 6F90010006   */ IL_013E: callvirt instance void PlayerController::SetTurningMode(valuetype InputController/TurningMode)
//    /* 0x000247E3 2853010006   */ IL_0143: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247E8 6FCF010006   */ IL_0148: callvirt instance void PlayerController::SetSkaterToMaster()
//    /* 0x000247ED 2853010006   */ IL_014D: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247F2 6F55010006   */ IL_0152: callvirt instance bool PlayerController::get_IsRespawning()
//    /* 0x000247F7 2D14         */ IL_0157: brtrue.s IL_016D

//    /* 0x000247F9 2853010006   */ IL_0159: call class PlayerController PlayerController::get_Instance()
//    /* 0x000247FE 725B0D0070   */ IL_015E: ldstr     "Extend"
//	  /* 0x00024803 229A99993E   */ IL_0163: ldc.r4    0.3
//	  /* 0x00024808 6F96010006   */ IL_0168: callvirt instance void PlayerController::CrossFadeAnimation(string, float32)

//    /* 0x0002480D 19           */ IL_016D: ldc.i4.3
//	  /* 0x0002480E 8D17000001   */ IL_016E: newarr[mscorlib] System.Object
//    /* 0x00024813 0A           */ IL_0173: stloc.0
//    /* 0x00024814 06           */ IL_0174: ldloc.0
//    /* 0x00024815 16           */ IL_0175: ldc.i4.0
//    /* 0x00024816 17           */ IL_0176: ldc.i4.1
//    /* 0x00024817 8C14010001   */ IL_0177: box[mscorlib] System.Boolean
//    /* 0x0002481C A2           */ IL_017C: stelem.ref
//    /* 0x0002481D 06           */ IL_017D: ldloc.0
//    /* 0x0002481E 17           */ IL_017E: ldc.i4.1
//    /* 0x0002481F 16           */ IL_017F: ldc.i4.0                                                     // <------ Here is where the bool for canGrind is set  ->  patched to ldc.i4.1 which sets 1(true) instead of 0(false)
//    /* 0x00024820 8C14010001   */ IL_0180: box[mscorlib] System.Boolean
//    /* 0x00024825 A2           */ IL_0185: stelem.ref
//    /* 0x00024826 06           */ IL_0186: ldloc.0
//    /* 0x00024827 18           */ IL_0187: ldc.i4.2
//    /* 0x00024828 02           */ IL_0188: ldarg.0
//    /* 0x00024829 7BB6030004   */ IL_0189: ldfld     class Dreamteck.Splines.SplineComputer PlayerState_Grinding::_spline
//    /* 0x0002482E A2           */ IL_018E: stelem.ref
//    /* 0x0002482F 02           */ IL_018F: ldarg.0
//    /* 0x00024830 D04B000002   */ IL_0190: ldtoken PlayerState_InAir
//    /* 0x00024835 289F01000A   */ IL_0195: call      class [mscorlib]System.Type[mscorlib] System.Type::GetTypeFromHandle(valuetype[mscorlib] System.RuntimeTypeHandle)
//    /* 0x0002483A 06           */ IL_019A: ldloc.0
//	  /* 0x0002483B 2845110006   */ IL_019B: call instance bool FSMHelper.BaseFSMState::DoTransition(class [mscorlib] System.Type, object[])
//	  /* 0x00024840 26           */ IL_01A0: pop
//    /* 0x00024841 2A           */ IL_01A1: ret
//} // end of method PlayerState_Grinding::OnGrindEnded
