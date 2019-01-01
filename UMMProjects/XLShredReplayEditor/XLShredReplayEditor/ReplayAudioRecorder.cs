
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor
{

    // Token: 0x02000231 RID: 561
    public class ReplayAudioRecorder : MonoBehaviour {
        // Token: 0x0600172F RID: 5935 RVA: 0x00073AB8 File Offset: 0x00071CB8
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

        // Token: 0x06001730 RID: 5936 RVA: 0x00073B64 File Offset: 0x00071D64
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

        // Token: 0x170005AC RID: 1452
        // (get) Token: 0x06001732 RID: 5938 RVA: 0x00011A70 File Offset: 0x0000FC70
        private float playBackTime {
            get {
                return ReplayManager.Instance.playbackTime;
            }
        }

        // Token: 0x04001133 RID: 4403
        public List<ReplayAudioRecorder.RecordedSoundFrame> recordedSoundFrames;

        // Token: 0x04001134 RID: 4404
        private double time;

        // Token: 0x04001135 RID: 4405
        public bool isRecording;

        // Token: 0x04001136 RID: 4406
        public bool isPayBack;

        // Token: 0x02000232 RID: 562
        public struct RecordedSoundFrame {
            // Token: 0x04001137 RID: 4407
            public float[] data;

            // Token: 0x04001138 RID: 4408
            public int channel;

            // Token: 0x04001139 RID: 4409
            public float time;
        }
    }

}
