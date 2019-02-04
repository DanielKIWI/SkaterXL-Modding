using System;
using UnityEngine;
using Harmony12;

namespace XLShredSessionMarkerManager.Patches {
    [HarmonyPatch(typeof(AssetBundle))]
    [HarmonyPatch("LoadFromFile")]
    [HarmonyPatch(new Type[] { typeof(string)})]
    static class AssetBundle_LoadFromFile {
        static void Postfix(AssetBundle __result, string path) {
            if (!Main.enabled) return;
            if (__result != null) {
                Main.LoadedAssetBundle(__result, path);
            }
        }
    }
}
