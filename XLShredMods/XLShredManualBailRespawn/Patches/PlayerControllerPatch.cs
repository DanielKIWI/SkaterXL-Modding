using System;
using System.Collections.Generic;
using System.Collections;
using Harmony12;
using Rewired;
using XLShredLib;
using UnityEngine;

namespace XLShredManualBailRespawn.Patches {
    [HarmonyPatch(typeof(PlayerController))]
    [HarmonyPatch("DoBailDelay")]
    static class PlayerControllerPatch {

        static Traverse respawnData = Traverse.Create(Traverse.CreateWithType("XLShredRespawnNearBail.Extensions.RespawnExtensions, XLShredRespawnNearBail, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").Method("GetExtensionComponent", new Type[] { typeof(Respawn) }).GetValue(new[] { PlayerController.Instance.respawn }));

        public static void Postfix(PlayerController __instance) {
            if (!Main.enabled) return;
            __instance.CancelInvoke("DoBail");

            // Respawn Near Bail Compatibility
            bool bailNearRespawnActive = XLShredDataRegistry.GetDataOrDefault<bool>("kiwi.XLShredRespawnNearBail", "isRespawnNearBailActive", false);
            if (bailNearRespawnActive) {
                __instance.StopCoroutine(respawnData.Field("DoBailTmpCoroutine").GetValue<Coroutine>());
            }

            __instance.StartCoroutine(CheckForRespawn(__instance, bailNearRespawnActive));
        }
        public static IEnumerator CheckForRespawn(PlayerController playerController, bool bailNearRespawnActive = false) {
            while (Main.enabled) {
                if (playerController.inputController.player.GetButtonDown("A")) {
                    if (!bailNearRespawnActive) {
                        playerController.DoBail();
                        yield break;
                    } else {
                        // Respawn Near Bail Compatibility
                        respawnData.Method("DoTmpRespawn").GetValue();
                        yield break;
                    }
                }
                if (playerController.inputController.player.GetAxis("DPadY") > 0.1f) {
                    yield break;
                }
                yield return null;
            }
        }
    }
}
