using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XLShredReplayEditor {
    class AudioSourceDataForwarder : MonoBehaviour {
        public delegate void AudioSourceDataReceiver(float[] data, int channels, AudioSource source);
        public AudioSourceDataReceiver receiver;
        public AudioSource audioSource;

        private void OnAudioFilterRead(float[] data, int channels) {
            if (receiver != null)
                receiver.Invoke(data, channels, audioSource);
        }
    }
}
