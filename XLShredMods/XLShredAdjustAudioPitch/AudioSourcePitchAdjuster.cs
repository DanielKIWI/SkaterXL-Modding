using System;
using System.Collections.Generic;
using UnityEngine;
using XLShredLib;

namespace XLShredAdjustAudioPitch {
    public class AudioSourcePitchAdjuster: MonoBehaviour {
        public AudioSource audioSource;
        public void Update() {
            bool replayEditorActive = XLShredDataRegistry.GetDataOrDefault<bool>("kiwi.XLShredReplayEditor", "isReplayEditorActive", false);
            if (!replayEditorActive) {
                audioSource.pitch = Time.timeScale;
            }
        }
    }
}
