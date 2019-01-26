using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Harmony12;
using Dreamteck.Splines;

namespace XLShredBetterVertGrind.Patches {
    [HarmonyPatch(typeof(PlayerState_Grinding))]
    [HarmonyPatch("OnGrindStay")]
    public static class PlayerState_GrindingPatches {
        private static Traverse<bool[]> _wheelDownField;
        public static Traverse<bool[]> WheelDownField {
            get {
                if (_wheelDownField == null) {
                    _wheelDownField = Traverse.Create(PlayerController.Instance.boardController).Field<bool[]>("_wheelsDown") ;
                }
                return _wheelDownField;
            }
        }
        public static void Postfix(PlayerState_Grinding __instance, SplineComputer ____spline) {
            var boardC = PlayerController.Instance.boardController;
            bool lFrontDown = WheelDownField.Value[0];
            bool rFrontDown = WheelDownField.Value[1];
            bool rBackDown = WheelDownField.Value[2];
            bool lBackDown = WheelDownField.Value[3];
            Debug.Log("Wheels Down: rb: " + rBackDown + ", lb: " + lBackDown + ", rf: " + rFrontDown + ", lf: " + lFrontDown);
            //if (boardC.triggerManager.IsColliding) {
            //    Debug.Log("BetterVertGrind: TriggerManager.IsColliding");
            //    switch (boardC.triggerManager.grindDetection.grindType) {
            //        case GrindDetection.GrindType.FsBoardSlide:
            //        case GrindDetection.GrindType.BsBoardSlide:
            //        case GrindDetection.GrindType.FsLipSlide:
            //        case GrindDetection.GrindType.BsLipSlide:
            //            if (boardC.AllDown) {
            //                Debug.Log("BoardSlide AllDown");
            //                PlayerController.Instance.AnimGrindTransition(false);
            //                PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
            //                PlayerController.Instance.SetBoardToMaster();
            //                if (!PlayerController.Instance.IsRespawning) {
            //                    PlayerController.Instance.CrossFadeAnimation("Riding", 0.3f);
            //                }
            //                __instance.DoTransition(typeof(PlayerState_Riding), null);
            //            } else if (rBackDown && lBackDown) {
            //                Debug.Log("BoardSlide BackTruckDown");
            //                __instance.DoTransition(typeof(PlayerState_Manualling));
            //            } else if (rFrontDown && lFrontDown) {
            //                Debug.Log("BoardSlide FrontTruckDown");
            //                __instance.DoTransition(typeof(PlayerState_Manualling));
            //            }
            //            break;
            //    }
            //}
            //PlayerController.Instance.SkaterRotation(true, false);
        }
    }
}
