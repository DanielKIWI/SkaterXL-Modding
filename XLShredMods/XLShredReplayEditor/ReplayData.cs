using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XLShredReplayEditor {
    [Serializable]
    public class ReplayData {
        public ReplayData() {
            List<ReplayRecordedFrame> list = ReplayManager.Instance.recorder.recordedFrames.FindAll((ReplayRecordedFrame f) => f.time >= ReplayManager.Instance.clipStartTime && f.time <= ReplayManager.Instance.clipEndTime);
            this.recordedFrames = list.ToArray();
            float time = this.recordedFrames[0].time;
            float time2 = this.recordedFrames[this.recordedFrames.Length - 1].time;
            this.recordedTime = time2 - time;
            for (int i = 0; i < this.recordedFrames.Length; i++) {
                this.recordedFrames[i].time -= time;
            }
            this.cameraKeyFrames = ReplayManager.Instance.cameraController.keyFrames.ToArray();
            for (int j = 0; j < this.cameraKeyFrames.Length; j++) {
                this.cameraKeyFrames[j].time -= time;
                if (this.cameraKeyFrames[j].time < 0f) {
                    this.cameraKeyFrames[j].time = 0f;
                }
                if (this.cameraKeyFrames[j].time > time2) {
                    this.cameraKeyFrames[j].time = time2;
                }
            }
        }
        
        public void Load() {
            ReplayManager.Instance.recorder.recordedFrames = new List<ReplayRecordedFrame>(this.recordedFrames);
            ReplayManager.Instance.clipStartTime = 0f;
            ReplayManager.Instance.clipEndTime = this.recordedTime;
            ReplayManager.Instance.playbackTime = 0f;
            ReplayManager.Instance.previousFrameIndex = 0;
            float time = this.recordedFrames[0].time;
            float time2 = this.recordedFrames[this.recordedFrames.Length - 1].time;
            this.recordedTime = time2 - time;
            for (int i = 0; i < this.recordedFrames.Length; i++) {
                this.recordedFrames[i].time -= time;
            }
            ReplayManager.Instance.cameraController.keyFrames = new List<KeyFrame>(this.cameraKeyFrames);
        }
        
        public void SaveToFile(string path) {
            if (!path.EndsWith(".json")) {
                path += ".json";
            }
            string contents = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, contents);
        }


        public static void SaveCurrentToFile(string path) {
            new ReplayData().SaveToFile(path);
        }


        public static void LoadFromFile(string path) {
            JsonUtility.FromJson<ReplayData>(File.ReadAllText(path)).Load();
        }

        [SerializeField]
        public ReplayRecordedFrame[] recordedFrames;


        [SerializeField]
        public KeyFrame[] cameraKeyFrames;


        [SerializeField]
        public float recordedTime;
    }

}
