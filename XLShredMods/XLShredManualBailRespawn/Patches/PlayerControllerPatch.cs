using System;
using System.Collections.Generic;
using System.Collections;
using Harmony12;
using Rewired;

namespace XLShredManualBailRespawn.Patches {
    [HarmonyPatch(typeof(PlayerController))]
    [HarmonyPatch("DoBailDelay")]
    static class PlayerControllerPatch {
        public static void Postfix(PlayerController __instance) {
            if (!Main.enabled) return;
            __instance.CancelInvoke("DoBail");
            __instance.StartCoroutine(CheckForRespawn(__instance));
        }
        public static IEnumerator CheckForRespawn(PlayerController playerController) {
            while (Main.enabled) {
                if (playerController.inputController.player.GetButtonDown("A")) {
                    playerController.DoBail();
                    yield break;
                }
                if (playerController.inputController.player.GetAxis("DPadY") > 0.1f) {
                    yield break;
                }
                yield return null;
            }
        }
    }
}
