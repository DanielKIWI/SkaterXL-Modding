using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Harmony12;
using Rewired;
using UnityEngine;

namespace XLShredRealisticTrucks.Patches {
    [HarmonyPatch(typeof(BoardController))]
    [HarmonyPatch("ApplyFriction")]
    public static class BoardController_ApplyFriction_Patch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr) {
            yield break;
        }
        public static void Postfix(BoardController __instance, ref Rigidbody ___frontTruckRigidbody, ref Rigidbody ___backTruckRigidbody) {
            ApplyFrictionToTruck(___frontTruckRigidbody, !__instance.IsBoardBackwards);
            ApplyFrictionToTruck(___backTruckRigidbody, __instance.IsBoardBackwards);
        }
        public static void ApplyFrictionToTruck(Rigidbody truck, bool isFrontTruck) {
            Vector3 localVel = truck.transform.InverseTransformDirection(truck.velocity);
            localVel.y = 0;
            float sideWayFriction = isFrontTruck ? Main.currentFrontWheelsFriction: Main.currentBackWheelsFriction;
            Vector3 targetVel = (localVel.z > 0 ? Vector3.forward : Vector3.back)* localVel.magnitude;
            Vector3 dv = targetVel - localVel;

            //Debug.Log("relativeVelocity for truck" + truck.name + ": " + vector + ", force: " + vector2);
            truck.AddForce(truck.transform.TransformDirection(dv * sideWayFriction), ForceMode.VelocityChange);
            truck.AddForce(truck.transform.TransformDirection(-localVel * Main.settings.RollFriction), ForceMode.VelocityChange);
        }
    }

    [HarmonyPatch(typeof(BoardController))]
    [HarmonyPatch("AddTurnTorque")]
    public static class BoardController_AddTurnTorque_Patch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (CodeInstruction inst in instructions) {
                if (inst.opcode == OpCodes.Call && (MethodInfo)inst.operand == AccessTools.Method(typeof(BoardController), "set_TurnTarget")) {
                    yield return inst;
                    break;
                }
                yield return inst;
            }
        }
    }
    [HarmonyPatch(typeof(BoardController))]
    [HarmonyPatch("RemoveTurnTorqueLinear")]
    public static class BoardController_RemoveTurnTorqueLinear_Patch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (CodeInstruction inst in instructions) {
                if (inst.opcode == OpCodes.Call && (MethodInfo)inst.operand == AccessTools.Method(typeof(BoardController), "set_TurnTarget")) {
                    yield return inst;
                    break;
                }
                yield return inst;
            }
        }
    }
    [HarmonyPatch(typeof(BoardController))]
    [HarmonyPatch("RemoveTurnTorque")]
    public static class BoardController_RemoveTurnTorque_Patch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (CodeInstruction inst in instructions) {
                if (inst.opcode == OpCodes.Call && (MethodInfo)inst.operand == AccessTools.Method(typeof(BoardController), "set_TurnTarget")) {
                    yield return inst;
                    break;
                }
                yield return inst;
            }
        }
    }
    [HarmonyPatch(typeof(BoardController))]
    [HarmonyPatch("FixedUpdate")]
    public static class BoardController_FixedUpdate_Patch {
        public static bool Prefix(BoardController __instance, Rigidbody ___backTruckRigidbody, Rigidbody ___frontTruckRigidbody, Rigidbody ___boardRigidbody, TriggerManager ___triggerManager, bool ____grounded) {
            if (PlayerController.Instance.movementMaster == PlayerController.MovementMaster.Board && ____grounded && !___triggerManager.IsColliding) {
                Vector3 vector = __instance.boardTransform.InverseTransformDirection(__instance.boardRigidbody.velocity);
                bool lsb = PlayerController.Instance.inputController.player.GetButton("Right Stick Button");
                bool rsb = PlayerController.Instance.inputController.player.GetButton("Left Stick Button");
                float leftTruckPower = Mathf.Lerp(Main.settings.RollSideWaysFriction, Main.settings.PowerSlideFriction, lsb ? 1f : (rsb ? 0.5f : 0f));
                float rightTruckPower = Mathf.Lerp(Main.settings.RollSideWaysFriction, Main.settings.PowerSlideFriction, lsb ? 1f : (rsb ? 0.5f : 0f));
                Main.currentFrontWheelsFriction += ((__instance.IsBoardBackwards ? leftTruckPower : rightTruckPower) - Main.currentFrontWheelsFriction) * 0.2f;
                Main.currentBackWheelsFriction += ((__instance.IsBoardBackwards ? rightTruckPower : leftTruckPower) - Main.currentBackWheelsFriction) * 0.2f;
                //Debug.Log(string.Concat(new object[]
                //{
                //"Board Velocity: ",
                //__instance.boardRigidbody.velocity,
                //", Local Velocity: ",
                //vector
                //}));
                //Debug.Log(string.Concat(new object[]
                //{
                //"BackTruckPos: ",
                //___backTruckRigidbody.position - ___boardRigidbody.position,
                //", magnitude: ",
                //(___backTruckRigidbody.position - ___boardRigidbody.position).magnitude
                //}));
                //Debug.Log(string.Concat(new object[]
                //{
                //"FrontTruckPos: ",
                //___frontTruckRigidbody.position - ___boardRigidbody.position,
                //", magnitude: ",
                //(___frontTruckRigidbody.position - ___boardRigidbody.position).magnitude
                //}));
            }
            return true;

        }
    }
}


//              ApplyFriction per Wheel

//    Transform[] array = new Transform[]
//    {
//    ____wheel1,
//    ____wheel2,
//    ____wheel3,
//    ____wheel4
//    };
//    for (int i = 0; i < 4; i++) {
//        if (____wheelsDown[i]) {
//            Transform transform = array[i];
//            transform.InverseTransformDirection(___boardRigidbody.GetRelativePointVelocity(array[i].position - ___boardRigidbody.position));
//            Vector3 vector = transform.InverseTransformDirection(___boardRigidbody.GetRelativePointVelocity(array[i].position - ___boardRigidbody.position));
//            Vector3 vector2 = -vector;
//            vector2.x *= ((___IsBoardBackwards ? (i > 2) : (i < 3)) ? ___currentFrontWheelsFriction : ___currentBackWheelsFriction);
//            vector2.y = 0f;
//            vector2.z *= ___RollFriction;
//            vector2 /= 4f;
//            Debug.Log(string.Concat(new object[]
//            {
//            "relativeVelocity for wheel ",
//            i,
//            ": ",
//            vector,
//            ", force: ",
//            vector2
//            }));
//            ___boardRigidbody.AddForceAtPosition(___boardTransform.TransformDirection(vector2), array[i].position, ForceMode.VelocityChange);
//        }
//    }



//            this.PowerSlideFriction = 0.1f;
//        this.RollSideWaysFriction = 0.7f;
//        this.RollFriction = 0f;
//}
//        this.lastFloatFieldValues = new Dictionary<string, float>();
//        this.lastVector3FieldValues = new Dictionary<string, Vector3>();
