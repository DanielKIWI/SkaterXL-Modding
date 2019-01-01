using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XLShredReplayEditor {

    // Token: 0x0200022F RID: 559
    [Serializable]
    public class ReplayData {
        // Token: 0x06001727 RID: 5927 RVA: 0x00073878 File Offset: 0x00071A78
        public ReplayData() {
            List<ReplaySkaterState> list = ReplayManager.Instance.recorder.recordedFrames.FindAll((ReplaySkaterState f) => f.time >= ReplayManager.Instance.clipStartTime && f.time <= ReplayManager.Instance.clipEndTime);
            this.recordedFrames = list.ToArray();
            float time = this.recordedFrames[0].time;
            float time2 = this.recordedFrames[this.recordedFrames.Length - 1].time;
            this.recordedTime = time2 - time;
            for (int i = 0; i < this.recordedFrames.Length; i++) {
                this.recordedFrames[i].time -= time;
            }
            this.cameraKeyStones = ReplayManager.Instance.cameraController.keyStones.ToArray();
            for (int j = 0; j < this.cameraKeyStones.Length; j++) {
                this.cameraKeyStones[j].time -= time;
                if (this.cameraKeyStones[j].time < 0f) {
                    this.cameraKeyStones[j].time = 0f;
                }
                if (this.cameraKeyStones[j].time > time2) {
                    this.cameraKeyStones[j].time = time2;
                }
            }
        }

        // Token: 0x06001728 RID: 5928 RVA: 0x000739B0 File Offset: 0x00071BB0
        public void Load() {
            ReplayManager.Instance.recorder.recordedFrames = new List<ReplaySkaterState>(this.recordedFrames);
            ReplayManager.Instance.clipStartTime = 0f;
            ReplayManager.Instance.clipEndTime = this.recordedTime;
            ReplayManager.Instance.playbackTime = 0f;
            ReplayManager.Instance.previousFrame = 0;
            float time = this.recordedFrames[0].time;
            float time2 = this.recordedFrames[this.recordedFrames.Length - 1].time;
            this.recordedTime = time2 - time;
            for (int i = 0; i < this.recordedFrames.Length; i++) {
                this.recordedFrames[i].time -= time;
            }
            ReplayManager.Instance.cameraController.keyStones = new List<ReplayCameraController.KeyStone>(this.cameraKeyStones);
        }

        // Token: 0x06001729 RID: 5929 RVA: 0x00073A80 File Offset: 0x00071C80
        public void SaveToFile(string path) {
            if (!path.EndsWith(".json")) {
                path += ".json";
            }
            string contents = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, contents);
        }

        // Token: 0x0600172A RID: 5930 RVA: 0x00011A1A File Offset: 0x0000FC1A
        public static void SaveCurrentToFile(string path) {
            new ReplayData().SaveToFile(path);
        }

        // Token: 0x0600172B RID: 5931 RVA: 0x00011A27 File Offset: 0x0000FC27
        public static void LoadFromFile(string path) {
            JsonUtility.FromJson<ReplayData>(File.ReadAllText(path)).Load();
        }

        // Token: 0x0400112E RID: 4398
        [SerializeField]
        public ReplaySkaterState[] recordedFrames;

        // Token: 0x0400112F RID: 4399
        [SerializeField]
        public ReplayCameraController.KeyStone[] cameraKeyStones;

        // Token: 0x04001130 RID: 4400
        [SerializeField]
        public float recordedTime;
    }

}
