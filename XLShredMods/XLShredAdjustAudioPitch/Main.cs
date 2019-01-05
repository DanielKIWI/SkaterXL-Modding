using UnityEngine;
using System.Collections.Generic;
using UnityModManagerNet;
using System;

using XLShredLib;
using XLShredLib.UI;

namespace XLShredAdjustAudioPitch {

    static class Main {
        public static List<AudioSourcePitchAdjuster> pitchAdjusters;
        public static bool enabled;
        public static String modId;
        // Send a response to the mod manager about the launch status, success or not.
        static void Load(UnityModManager.ModEntry modEntry) {
            modId = modEntry.Info.Id;
            modEntry.OnToggle = OnToggle;
            pitchAdjusters = new List<AudioSourcePitchAdjuster>();

            ModUIBox uiBoxKiwi = ModMenu.Instance.RegisterModMaker("com.kiwi", "Kiwi");
            uiBoxKiwi.AddToggle("Adjust Audio Pitch corresponding to TimeScale", Side.left, () => enabled, true, (toggle) => {
                OnToggle(modEntry, toggle);
            });
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {
                foreach (AudioSource audioSource in GameObject.FindObjectsOfType<AudioSource>()) {
                    AudioSourcePitchAdjuster pa = audioSource.gameObject.AddComponent<AudioSourcePitchAdjuster>();
                    pa.audioSource = audioSource;
                    pitchAdjusters.Add(pa);
                }
                
            } else {
                foreach (AudioSourcePitchAdjuster pitchAdjuster in pitchAdjusters) {
                    GameObject.Destroy(pitchAdjuster);
                }
                pitchAdjusters.Clear();
            }
            return true;
        }
    }
}