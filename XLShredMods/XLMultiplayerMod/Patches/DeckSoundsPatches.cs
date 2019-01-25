using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony12;

namespace XLMultiplayerMod.Patches {
    [HarmonyPatch(typeof(DeckSounds))]
    [HarmonyPatch("Update")]
    public static class DeckSoundsPatches {
        public static void Postfix() {
            //Check if volume, pitch, mute or loop changed
            //If yes tell MultiplayerAudioManager
            //AudioClipRecorder OnAudioSourceValueChanged()
        }
    }
}
