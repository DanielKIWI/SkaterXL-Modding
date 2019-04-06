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
            var canGrindCode = codes[codes.Count - 10]; 
            //That line represents the value to that PlayerState_Air._canGrind gets set.
            //95 = 116 - 13 - 8,  Explanation: boolean value gets pushed to the stack at line 116 in OnGrindEnded IL code, function code starts at 13, 8 empty lines 
            //88 = 107 - 13 - 6  //for NEW patch 0.0.3
            //Count - 10, Explenation: the boolean is set 10 codeInstructions before the end //for NEW patch 0.0.5
            canGrindCode.opcode = OpCodes.Ldc_I4_1;     //Setting bool for PlayerState_Air._canGrind 
            return codes.AsEnumerable();
        }
    }
}


// PlayerState_Grinding.OnGrindEnded as C# code (for SkaterXL version 0.0.5)

// Token: 0x06000396 RID: 918 RVA: 0x00026494 File Offset: 0x00024694
//public override void OnGrindEnded() {
//PlayerController.Instance.boardController.boardRigidbody.velocity = this._lastVelocity;
//PlayerController.Instance.boardController.backTruckRigidbody.velocity = this._lastVelocity;
//PlayerController.Instance.boardController.frontTruckRigidbody.velocity = this._lastVelocity;
//PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
//PlayerController.Instance.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
//PlayerController.Instance.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
//PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
//	if (PlayerController.Instance.TwoWheelsDown())
//    {
//    PlayerController.Instance.AnimGrindTransition(false);
//    PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
//    PlayerController.Instance.SetBoardToMaster();
//    if (!PlayerController.Instance.IsRespawning) {
//        PlayerController.Instance.CrossFadeAnimation("Riding", 0.3f);
//    }
//    base.DoTransition(typeof(PlayerState_Riding), null);
//    return;
//}
//PlayerController.Instance.AnimGrindTransition(false);
//PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
//PlayerController.Instance.SetSkaterToMaster();
//	if (!PlayerController.Instance.IsRespawning)
//    {
//    PlayerController.Instance.CrossFadeAnimation("Extend", 0.3f);
//}
//Vector3 force = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
//PlayerController.Instance.skaterController.skaterRigidbody.AddForce(force, ForceMode.Impulse);
//	object[]
//args = new object[]
//	{
//		true,
//      false,                                                    //<-------   This bool represents canGrind
//	};
//	base.DoTransition(typeof(PlayerState_InAir), args);
//}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// PlayerState_Grinding.OnGrindEnded as C# code

//    // Token: 0x060003BC RID: 956 RVA: 0x00028BDC File Offset: 0x00026DDC
//.method public hidebysig virtual
//    instance void OnGrindEnded() cil managed {
//	// Header Size: 12 bytes
//	// Code Size: 439 (0x1B7) bytes
//	// LocalVarSig Token: 0x1100004B RID: 75
//	.maxstack 3
//	.locals init (
//		[0]
//    valuetype [UnityEngine.CoreModule]
//    UnityEngine.Vector3,
//		[1] object[]
//	)

//	/* 0x00026DE8 2858010006   */ IL_0000: call      class PlayerController PlayerController::get_Instance()
//    /* 0x00026DED 7B03020004   */ IL_0005: ldfld class BoardController PlayerController::boardController
///* 0x00026DF2 7B40000004   */ IL_000A: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody BoardController::boardRigidbody
//    /* 0x00026DF7 02           */ IL_000F: ldarg.0
//    /* 0x00026DF8 7BD0030004   */ IL_0010: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 PlayerState_Grinding::_lastVelocity
///* 0x00026DFD 6F6C00000A   */ IL_0015: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_velocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E02 2858010006   */
//                                  IL_001A: call class PlayerController PlayerController::get_Instance()
///* 0x00026E07 7B03020004   */ IL_001F: ldfld class BoardController PlayerController::boardController
///* 0x00026E0C 7B42000004   */ IL_0024: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody BoardController::backTruckRigidbody
//    /* 0x00026E11 02           */ IL_0029: ldarg.0
//    /* 0x00026E12 7BD0030004   */ IL_002A: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 PlayerState_Grinding::_lastVelocity
///* 0x00026E17 6F6C00000A   */ IL_002F: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_velocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E1C 2858010006   */
//                                  IL_0034: call class PlayerController PlayerController::get_Instance()
///* 0x00026E21 7B03020004   */ IL_0039: ldfld class BoardController PlayerController::boardController
///* 0x00026E26 7B41000004   */ IL_003E: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody BoardController::frontTruckRigidbody
//    /* 0x00026E2B 02           */ IL_0043: ldarg.0
//    /* 0x00026E2C 7BD0030004   */ IL_0044: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 PlayerState_Grinding::_lastVelocity
///* 0x00026E31 6F6C00000A   */ IL_0049: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_velocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E36 2858010006   */
//                                  IL_004E: call class PlayerController PlayerController::get_Instance()
///* 0x00026E3B 7B03020004   */ IL_0053: ldfld class BoardController PlayerController::boardController
///* 0x00026E40 7B40000004   */ IL_0058: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody BoardController::boardRigidbody
//    /* 0x00026E45 288000000A   */ IL_005D: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
///* 0x00026E4A 6F8100000A   */ IL_0062: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E4F 2858010006   */
//                                  IL_0067: call class PlayerController PlayerController::get_Instance()
///* 0x00026E54 7B03020004   */ IL_006C: ldfld class BoardController PlayerController::boardController
///* 0x00026E59 7B42000004   */ IL_0071: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody BoardController::backTruckRigidbody
//    /* 0x00026E5E 288000000A   */ IL_0076: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
///* 0x00026E63 6F8100000A   */ IL_007B: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E68 2858010006   */
//                                  IL_0080: call class PlayerController PlayerController::get_Instance()
///* 0x00026E6D 7B03020004   */ IL_0085: ldfld class BoardController PlayerController::boardController
///* 0x00026E72 7B41000004   */ IL_008A: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody BoardController::frontTruckRigidbody
//    /* 0x00026E77 288000000A   */ IL_008F: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
///* 0x00026E7C 6F8100000A   */ IL_0094: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E81 2858010006   */
//                                  IL_0099: call class PlayerController PlayerController::get_Instance()
///* 0x00026E86 7B02020004   */ IL_009E: ldfld class SkaterController PlayerController::skaterController
///* 0x00026E8B 7B6E020004   */ IL_00A3: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody SkaterController::skaterRigidbody
//    /* 0x00026E90 288000000A   */ IL_00A8: call valuetype [UnityEngine.CoreModule]UnityEngine.Vector3[UnityEngine.CoreModule] UnityEngine.Vector3::get_zero()
///* 0x00026E95 6F8100000A   */ IL_00AD: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::set_angularVelocity(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026E9A 2858010006   */
//                                  IL_00B2: call class PlayerController PlayerController::get_Instance()
///* 0x00026E9F 6FFC010006   */ IL_00B7: callvirt instance bool PlayerController::TwoWheelsDown()
//    /* 0x00026EA4 2C53         */
//                                  IL_00BC: brfalse.s IL_0111

//    /* 0x00026EA6 2858010006   */
//                                  IL_00BE: call class PlayerController PlayerController::get_Instance()
///* 0x00026EAB 16           */ IL_00C3: ldc.i4.0
//	/* 0x00026EAC 6FC6010006   */ IL_00C4: callvirt instance void PlayerController::AnimGrindTransition(bool)
//    /* 0x00026EB1 2858010006   */
//                                  IL_00C9: call class PlayerController PlayerController::get_Instance()
///* 0x00026EB6 16           */ IL_00CE: ldc.i4.0
//	/* 0x00026EB7 6F94010006   */ IL_00CF: callvirt instance void PlayerController::SetTurningMode(valuetype InputController/TurningMode)
//    /* 0x00026EBC 2858010006   */
//                                  IL_00D4: call class PlayerController PlayerController::get_Instance()
///* 0x00026EC1 6FD4010006   */ IL_00D9: callvirt instance void PlayerController::SetBoardToMaster()
//    /* 0x00026EC6 2858010006   */
//                                  IL_00DE: call class PlayerController PlayerController::get_Instance()
///* 0x00026ECB 6F5A010006   */ IL_00E3: callvirt instance bool PlayerController::get_IsRespawning()
//    /* 0x00026ED0 2D14         */
//                                  IL_00E8: brtrue.s IL_00FE

//    /* 0x00026ED2 2858010006   */
//                                  IL_00EA: call class PlayerController PlayerController::get_Instance()
///* 0x00026ED7 72ED010070   */ IL_00EF: ldstr     "Riding"
//	/* 0x00026EDC 229A99993E   */ IL_00F4: ldc.r4    0.3
//	/* 0x00026EE1 6F9A010006   */ IL_00F9: callvirt instance void PlayerController::CrossFadeAnimation(string, float32)

//    /* 0x00026EE6 02           */
//                                  IL_00FE: ldarg.0
//	/* 0x00026EE7 D056000002   */ IL_00FF: ldtoken PlayerState_Riding
//    /* 0x00026EEC 28CF01000A   */
//                                  IL_0104: call class [mscorlib]
//System.Type[mscorlib] System.Type::GetTypeFromHandle(valuetype[mscorlib] System.RuntimeTypeHandle)
//    /* 0x00026EF1 14           */
//                                  IL_0109: ldnull
//    /* 0x00026EF2 28FB110006   */ IL_010A: call instance bool FSMHelper.BaseFSMState::DoTransition(class [mscorlib] System.Type, object[])
//	/* 0x00026EF7 26           */ IL_010F: pop
//    /* 0x00026EF8 2A           */ IL_0110: ret

//    /* 0x00026EF9 2858010006   */ IL_0111: call class PlayerController PlayerController::get_Instance()
///* 0x00026EFE 16           */ IL_0116: ldc.i4.0
//	/* 0x00026EFF 6FC6010006   */ IL_0117: callvirt instance void PlayerController::AnimGrindTransition(bool)
//    /* 0x00026F04 2858010006   */
//                                  IL_011C: call class PlayerController PlayerController::get_Instance()
///* 0x00026F09 18           */ IL_0121: ldc.i4.2
//	/* 0x00026F0A 6F94010006   */ IL_0122: callvirt instance void PlayerController::SetTurningMode(valuetype InputController/TurningMode)
//    /* 0x00026F0F 2858010006   */
//                                  IL_0127: call class PlayerController PlayerController::get_Instance()
///* 0x00026F14 6FD5010006   */ IL_012C: callvirt instance void PlayerController::SetSkaterToMaster()
//    /* 0x00026F19 2858010006   */
//                                  IL_0131: call class PlayerController PlayerController::get_Instance()
///* 0x00026F1E 6F5A010006   */ IL_0136: callvirt instance bool PlayerController::get_IsRespawning()
//    /* 0x00026F23 2D14         */
//                                  IL_013B: brtrue.s IL_0151

//    /* 0x00026F25 2858010006   */
//                                  IL_013D: call class PlayerController PlayerController::get_Instance()
///* 0x00026F2A 72D5110070   */ IL_0142: ldstr     "Extend"
//	/* 0x00026F2F 229A99993E   */ IL_0147: ldc.r4    0.3
//	/* 0x00026F34 6F9A010006   */ IL_014C: callvirt instance void PlayerController::CrossFadeAnimation(string, float32)

//    /* 0x00026F39 2858010006   */
//                                  IL_0151: call class PlayerController PlayerController::get_Instance()
///* 0x00026F3E 7B02020004   */ IL_0156: ldfld class SkaterController PlayerController::skaterController
///* 0x00026F43 2858010006   */ IL_015B: call class PlayerController PlayerController::get_Instance()
///* 0x00026F48 7B02020004   */ IL_0160: ldfld class SkaterController PlayerController::skaterController
///* 0x00026F4D 7B6E020004   */ IL_0165: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody SkaterController::skaterRigidbody
//    /* 0x00026F52 6F5900000A   */ IL_016A: callvirt instance valuetype[UnityEngine.CoreModule] UnityEngine.Vector3[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::get_velocity()
///* 0x00026F57 6F54020006   */ IL_016F: callvirt instance valuetype[UnityEngine.CoreModule] UnityEngine.Vector3 SkaterController::PredictLanding(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3)
//    /* 0x00026F5C 0A           */
//                                  IL_0174: stloc.0
//	/* 0x00026F5D 2858010006   */ IL_0175: call class PlayerController PlayerController::get_Instance()
///* 0x00026F62 7B02020004   */ IL_017A: ldfld class SkaterController PlayerController::skaterController
///* 0x00026F67 7B6E020004   */ IL_017F: ldfld class [UnityEngine.PhysicsModule]
//UnityEngine.Rigidbody SkaterController::skaterRigidbody
//    /* 0x00026F6C 06           */ IL_0184: ldloc.0
//    /* 0x00026F6D 17           */ IL_0185: ldc.i4.1
//    /* 0x00026F6E 6F6F00000A   */ IL_0186: callvirt instance void[UnityEngine.PhysicsModule] UnityEngine.Rigidbody::AddForce(valuetype[UnityEngine.CoreModule] UnityEngine.Vector3, valuetype[UnityEngine.PhysicsModule] UnityEngine.ForceMode)
//    /* 0x00026F73 18           */
//                                  IL_018B: ldc.i4.2
//	/* 0x00026F74 8D17000001   */ IL_018C: newarr[mscorlib] System.Object
//    /* 0x00026F79 0B           */ IL_0191: stloc.1
//    /* 0x00026F7A 07           */ IL_0192: ldloc.1
//    /* 0x00026F7B 16           */ IL_0193: ldc.i4.0
//    /* 0x00026F7C 17           */ IL_0194: ldc.i4.1
//    /* 0x00026F7D 8CFC000001   */ IL_0195: box[mscorlib] System.Boolean
//    /* 0x00026F82 A2           */ IL_019A: stelem.ref
//    /* 0x00026F83 07           */ IL_019B: ldloc.1
//    /* 0x00026F84 17           */ IL_019C: ldc.i4.1
//    /* 0x00026F85 16           */ IL_019D: ldc.i4.0                                               // <------ Here is where the bool for canGrind is set  ->  patched to ldc.i4.1 which sets 1(true) instead of 0(false)
//    /* 0x00026F86 8CFC000001   */ IL_019E: box[mscorlib] System.Boolean
//    /* 0x00026F8B A2           */ IL_01A3: stelem.ref
//    /* 0x00026F8C 02           */ IL_01A4: ldarg.0
//    /* 0x00026F8D D04D000002   */ IL_01A5: ldtoken PlayerState_InAir
//    /* 0x00026F92 28CF01000A   */ IL_01AA: call      class [mscorlib]
//System.Type[mscorlib] System.Type::GetTypeFromHandle(valuetype[mscorlib] System.RuntimeTypeHandle)
//    /* 0x00026F97 07           */
//                                  IL_01AF: ldloc.1
//	/* 0x00026F98 28FB110006   */ IL_01B0: call instance bool FSMHelper.BaseFSMState::DoTransition(class [mscorlib] System.Type, object[])
//	/* 0x00026F9D 26           */ IL_01B5: pop
//    /* 0x00026F9E 2A           */ IL_01B6: ret
//} // end of method PlayerState_Grinding::OnGrindEnded
