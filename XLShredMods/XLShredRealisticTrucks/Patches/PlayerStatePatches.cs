using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony12;

namespace XLShredRealisticTrucks.Patches {
    [HarmonyPatch(typeof(PlayerState_Pushing))]
    [HarmonyPatch("FixedUpdate")]
    public static class PlayerState_Pushing_FixedUpdate_Patch {
        public static void Postfix(PlayerState_Pushing __instance) {
            if (PlayerController.Instance.IsGrounded()) {
                PlayerController.Instance.ApplyWeightOnBoard();
            }
        }
    }
}
namespace XLShredRealisticTrucks.Patches {
    [HarmonyPatch(typeof(PlayerState_Riding))]
    [HarmonyPatch("FixedUpdate")]
    public static class PlayerState_Riding_FixedUpdate_Patch {
        public static void Postfix(PlayerState_Riding __instance) {
            if (PlayerController.Instance.IsGrounded()) {
                PlayerController.Instance.ApplyWeightOnBoard();
            }
        }
    }
}
namespace XLShredRealisticTrucks.Patches {
    using Extensions;

    [HarmonyPatch(typeof(PlayerState_Manualling))]
    [HarmonyPatch("FixedUpdate")]
    public static class PlayerState_Manualling_FixedUpdate_Patch {
        public static void Postfix(PlayerState_Riding __instance, int ____manualSign) {
            float sign = (float)____manualSign;
            if (PlayerController.Instance.boardController.IsBoardBackwards) sign *= -1;
            if (PlayerController.Instance.IsGrounded()) {
                PlayerController.Instance.ApplyWeightOnBoard(Main.settings.ManualWeightOnBoardYOffset * sign);
            }
        }
    }
}
