using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XLShredReplayEditor {
    class AudioSourceDataForwarder : MonoBehaviour {
        public delegate void AudioSourceDataReceiver(float[] data, int channels, int timeSamples);
        public AudioSourceDataReceiver receiver;
        public AudioSource audioSource;
        //private int bufferTimeSamplesOffset;
        private object offsetLocker = new object();
        private int id;

        private void Start() {
            //bufferTimeSamplesOffset = ReplayAudioRecorder.Instance.bufferTimeSamples - audioSource.timeSamples;
            id = audioSource.GetInstanceID();
        }
        //private void Update() {
        //    lock (offsetLocker) {
        //        int tsDiff = ReplayAudioRecorder.Instance.bufferTimeSamples - audioSource.timeSamples;
        //        if (Mathf.Abs(tsDiff - bufferTimeSamplesOffset) > 10) {
        //            bufferTimeSamplesOffset = tsDiff;
        //        }
        //    }
        //}

        private void OnAudioFilterRead(float[] data, int channels) {
            if (receiver != null)
                receiver.Invoke(data, channels, id);
        }
    }
}
