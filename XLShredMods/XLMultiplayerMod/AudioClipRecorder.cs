using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLMultiplayerMod {
    public class AudioClipRecorder: MonoBehaviour {
        public AudioSource[] sourcesToRecord;
        public enum AudioSourceValue {
            volume, pitch, loop
        }

        public void OnAudioSourceValueChanged(AudioSource source, AudioSourceValue changedValue, object newValue) {

        }
    }
}
