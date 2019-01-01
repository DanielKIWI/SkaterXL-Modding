
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor
{
    public class ReplayAudioRecorder : MonoBehaviour {

        public ReplayAudioRecorder.RecordedSoundFrame GetSoundFrame(float time, int lastFrame) {
            if (lastFrame < 0) {
                lastFrame = 0;
            }
            if (lastFrame > this.recordedSoundFrames.Count - 2) {
                lastFrame = this.recordedSoundFrames.Count - 2;
            }
            int num = lastFrame;
            if (time < this.recordedSoundFrames[lastFrame].time) {
                while (num > 0 && this.recordedSoundFrames[num].time > time) {
                    num--;
                }
                return this.recordedSoundFrames[num];
            }
            while (num < this.recordedSoundFrames.Count - 2 && this.recordedSoundFrames[num + 1].time < time) {
                num++;
            }
            return this.recordedSoundFrames[num];
        }
        
        private void OnAudioFilterRead(float[] data, int channel) {
            if (this.isRecording) {
                ReplayAudioRecorder.RecordedSoundFrame recordedSoundFrame = new ReplayAudioRecorder.RecordedSoundFrame {
                    data = new float[data.Length],
                    channel = channel,
                    time = ReplayManager.Instance.recorder.endTime
                };
                Array.Copy(data, recordedSoundFrame.data, data.Length);
                this.recordedSoundFrames.Add(recordedSoundFrame);
                this.time += 1.0 / (double)AudioSettings.outputSampleRate;
                return;
            }
            bool flag = this.isPayBack;
        }
        
        private float playBackTime {
            get {
                return ReplayManager.Instance.playbackTime;
            }
        }
        
        public List<ReplayAudioRecorder.RecordedSoundFrame> recordedSoundFrames;
        
        private double time;
        
        public bool isRecording;
        
        public bool isPayBack;
        
        public struct RecordedSoundFrame {
            public float[] data;
            
            public int channel;
            
            public float time;
        }
    }

}
