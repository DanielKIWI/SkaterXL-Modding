using UnityEngine;
using Harmony12;
using System;

namespace DebugGUI.Patches {
    [HarmonyPatch(typeof(Logger))]
    [HarmonyPatch(new Type[] { typeof(ILogHandler) })]
    static class LoggerPatch {
        static void PostFix(Logger __instance, ILogHandler logHandler) {
        }
    }
}
