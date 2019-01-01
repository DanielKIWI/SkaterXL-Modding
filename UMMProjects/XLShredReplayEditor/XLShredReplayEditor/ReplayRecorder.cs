using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {
    // Token: 0x0200021B RID: 539
    public class ReplayRecorder : MonoBehaviour {
        // Token: 0x060016B2 RID: 5810 RVA: 0x00071448 File Offset: 0x0006F648
        public void Start() {
            List<Transform> list = new List<Transform>(PlayerController.Instance.respawn.getSpawn);
            foreach (object obj in Enum.GetValues(typeof(HumanBodyBones))) {
                HumanBodyBones humanBodyBones = (HumanBodyBones)obj;
                if (humanBodyBones >= HumanBodyBones.Hips && humanBodyBones < HumanBodyBones.LastBone) {
                    Transform boneTransform = PlayerController.Instance.animationController.skaterAnim.GetBoneTransform(humanBodyBones);
                    if (!(boneTransform == null)) {
                        list.Add(boneTransform);
                    }
                }
            }
            this.transformsToBeRecorded = list.ToArray();
            this.recSkin = new GUIStyle();
            this.recSkin.fontSize = 20;
            this.recSkin.normal.textColor = Color.red;
            this.recordedFrames = new List<ReplaySkaterState>();
            this.MaxRecordedTime = 300f;
            this.startTime = 0f;
            this.StartRecording();
        }

        // Token: 0x060016B3 RID: 5811 RVA: 0x0001150D File Offset: 0x0000F70D
        public void StartRecording() {
            if (this.isRecording) {
                return;
            }
            this.isRecording = true;
            this.ClearRecording();
            this.endTime = 0f;
            this.RecordFrame();
        }

        // Token: 0x060016B4 RID: 5812 RVA: 0x00011536 File Offset: 0x0000F736
        public void StopRecording() {
            if (!this.isRecording) {
                return;
            }
            this.isRecording = false;
        }

        // Token: 0x060016B5 RID: 5813 RVA: 0x00011548 File Offset: 0x0000F748
        private void ClearRecording() {
            this.recordedFrames.Clear();
            this.startTime = this.endTime;
        }

        // Token: 0x060016B6 RID: 5814 RVA: 0x0007154C File Offset: 0x0006F74C
        public void Update() {
            if (!this.isRecording) {
                return;
            }
            this.endTime += Time.deltaTime;
            this.RecordFrame();
            if (this.endTime > this.MaxRecordedTime) {
                this.startTime = this.endTime - this.MaxRecordedTime;
                while (this.recordedFrames.Count > 0 && this.recordedFrames[0].time < this.startTime) {
                    this.recordedFrames.RemoveAt(0);
                }
            }
        }

        // Token: 0x1700059F RID: 1439
        // (get) Token: 0x060016B7 RID: 5815 RVA: 0x00011561 File Offset: 0x0000F761
        public int frameCount {
            get {
                return this.recordedFrames.Count;
            }
        }

        // Token: 0x060016B8 RID: 5816 RVA: 0x0001156E File Offset: 0x0000F76E
        public void ApplyRecordedFrame(int frame) {
            this.recordedFrames[frame].ApplyTo(this.transformsToBeRecorded);
        }

        // Token: 0x060016B9 RID: 5817 RVA: 0x00011587 File Offset: 0x0000F787
        private void RecordFrame() {
            this.recordedFrames.Add(new ReplaySkaterState(this.transformsToBeRecorded, this.endTime));
        }

        // Token: 0x060016BA RID: 5818 RVA: 0x000715D0 File Offset: 0x0006F7D0
        public void OnGUI() {
            if (this.isRecording) {
                string text = "● Rec";
                Vector2 vector = this.recSkin.CalcSize(new GUIContent(text));
                GUI.Label(new Rect((float)Screen.width - vector.x - 10f, 10f, vector.x, vector.y), text, this.recSkin);
            }
        }

        // Token: 0x060016BB RID: 5819 RVA: 0x00071634 File Offset: 0x0006F834
        public int GetFrameIndex(float time, int lastFrame = 0) {
            if (lastFrame < 0) {
                lastFrame = 0;
            }
            if (lastFrame > this.recordedFrames.Count - 2) {
                lastFrame = this.recordedFrames.Count - 2;
            }
            int num = lastFrame;
            if (time < this.recordedFrames[lastFrame].time) {
                while (num > 0 && this.recordedFrames[num].time > time) {
                    num--;
                }
                return num;
            }
            while (num < this.recordedFrames.Count - 2 && this.recordedFrames[num + 1].time < time) {
                num++;
            }
            return num;
        }

        // Token: 0x060016BD RID: 5821 RVA: 0x000716C8 File Offset: 0x0006F8C8
        public void ApplyRecordedTime(int frameIndex, float time) {
            if (this.recordedFrames.Count != 0) {
                if (this.recordedFrames.Count == 1) {
                    this.recordedFrames[0].ApplyTo(this.transformsToBeRecorded);
                    return;
                }
                if (frameIndex > 0 && frameIndex + 1 < this.recordedFrames.Count) {
                    ReplaySkaterState.Lerp(this.recordedFrames[frameIndex], this.recordedFrames[frameIndex + 1], time).ApplyTo(this.transformsToBeRecorded);
                }
            }
        }

        // Token: 0x040010CF RID: 4303
        public Transform[] transformsToBeRecorded;

        // Token: 0x040010D0 RID: 4304
        public bool isRecording;

        // Token: 0x040010D1 RID: 4305
        public List<ReplaySkaterState> recordedFrames;

        // Token: 0x040010D2 RID: 4306
        public float endTime;

        // Token: 0x040010D3 RID: 4307
        private GUIStyle recSkin;

        // Token: 0x040010D4 RID: 4308
        public float startTime;

        // Token: 0x040010D5 RID: 4309
        public float MaxRecordedTime;
    }

}
