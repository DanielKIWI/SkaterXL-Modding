using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Harmony12;
using Rewired;
using UnityEngine;

namespace XLShredRealisticTrucks.Patches {
    using Extensions;

    [HarmonyPatch(typeof(PlayerController))]
    [HarmonyPatch("ApplyWeightOnBoard")]
    public static class PlayerController_Awake_Patch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            yield break;
        }
        public static void Postfix(PlayerController __instance) {
            __instance.ApplyWeightOnBoard(0f);
        }
    }
}