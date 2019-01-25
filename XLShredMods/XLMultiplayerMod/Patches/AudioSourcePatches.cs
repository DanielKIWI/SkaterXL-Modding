using System;
using System.Collections.Generic;
using Harmony12;
using UnityEngine;

namespace XLMultiplayerMod.Patches {
    [HarmonyPatch(typeof(AudioSource))]
    [HarmonyPatch("PlayOneShot")]
    public static class AudioSource_PlayOneShot_Patch{
        public static void Postfix(AudioClip clip, float volumeScale, AudioSource __instance) {
            Debug.Log("PlayOneShot " + clip.name + " with volume " + volumeScale + " on source " + __instance);
        }
    }

    [HarmonyPatch(typeof(AudioSource))]
    [HarmonyPatch("Play")]
    public static class AudioSource_Play_Patch {
        public static void Postfix(AudioSource __instance) {
            Debug.Log("Play while clip: " + __instance.clip.name + " with volume " + __instance.volume + " on source " + __instance);
        }
    }

    [HarmonyPatch(typeof(AudioSource))]
    [HarmonyPatch("Update")]
    public static class AudioSource_Update_Patch {
        public static void Postfix(AudioSource __instance) {
            Debug.Log("Play while clip: " + __instance.clip.name + " with volume " + __instance.volume + " on source " + __instance);
        }
    }
}
