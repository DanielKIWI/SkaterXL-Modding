using UnityEngine;
using Harmony12;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace DebugGUI.Patches {
    [HarmonyPatch(typeof(Logger))]
    [HarmonyPatch(new Type[] { typeof(ILogHandler) })]
    static class LoggerPatch {
        static void PostFix(Logger __instance, ILogHandler logHandler) {
        }
    }
}
