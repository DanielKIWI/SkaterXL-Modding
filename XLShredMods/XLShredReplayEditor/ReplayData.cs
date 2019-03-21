using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;

namespace XLShredReplayEditor {
    [Serializable]
    public class ReplayData {
        public ReplayData() {
            this.recordedFrames = ReplayManager.Instance.recorder.recordedFrames.ToArray();
            testFrame = recordedFrames[0];
            float startTime = this.recordedFrames[0].time;
            float endTime = this.recordedFrames[this.recordedFrames.Length - 1].time;
            this.recordedTime = endTime - startTime;
            for (int i = 0; i < this.recordedFrames.Length; i++) {
                this.recordedFrames[i].time -= startTime;
            }
            this.cameraKeyFrames = ReplayManager.Instance.cameraController.keyFrames
                .Where(k => k.time >= startTime && k.time <= endTime)
                .ToList();
            foreach (var keyFrame in cameraKeyFrames) {
                keyFrame.time -= startTime;
            }
            Debug.Log(this);
            Debug.Log(cameraKeyFrames.Count);
        }

        public void Load() {
            ReplayManager.Instance.recorder.LoadFrames(new List<ReplayRecordedFrame>(this.recordedFrames));
            ReplayManager.Instance.clipStartTime = 0f;
            ReplayManager.Instance.clipEndTime = this.recordedTime;
            ReplayManager.Instance.playbackTime = 0f;
            ReplayManager.Instance.previousFrameIndex = 0;
            ReplayManager.Instance.cameraController.LoadKeyFrames(this.cameraKeyFrames);
        }

        public void SaveToFile(string path) {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            string dataPath = path + "\\Data.replay";
            string contents = JsonUtility.ToJson(this, false); //TODO: //FIXME: //Es wird nur recordedTime gespeichert
            File.WriteAllText(dataPath, contents);

            string audioPath = path + "\\Audio.wav";
            ReplayManager.Instance.audioRecorder.WriteTmpStreamToPath(audioPath, ReplayManager.Instance.recorder.startTime, ReplayManager.Instance.recorder.endTime);
        }

        public static IEnumerator LoadFromFile(string path) {
            string audioPath = path + "\\Audio.wav";
            string dataPath = path + "\\Data.replay";
            JsonUtility.FromJson<ReplayData>(File.ReadAllText(dataPath)).Load();
            yield return ReplayManager.Instance.audioRecorder.LoadReplayAudio(audioPath);
        }
        
        public ReplayRecordedFrame testFrame;
        public ReplayRecordedFrame[] recordedFrames;
        
        public List<KeyFrame> cameraKeyFrames;

        
        public float recordedTime;
    }
    //[Serializable]
    //public class TransformJSON
}
