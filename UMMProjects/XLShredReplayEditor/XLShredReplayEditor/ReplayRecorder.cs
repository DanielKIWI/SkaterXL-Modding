﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {

    public class ReplayRecorder : MonoBehaviour {

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


        public void StartRecording() {
            if (this.isRecording) {
                return;
            }
            this.isRecording = true;
            this.ClearRecording();
            this.endTime = 0f;
            this.RecordFrame();
        }


        public void StopRecording() {
            if (!this.isRecording) {
                return;
            }
            this.isRecording = false;
        }


        private void ClearRecording() {
            this.recordedFrames.Clear();
            this.startTime = this.endTime;
        }


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



        public int frameCount {
            get {
                return this.recordedFrames.Count;
            }
        }


        public void ApplyRecordedFrame(int frame) {
            this.recordedFrames[frame].ApplyTo(this.transformsToBeRecorded);
        }


        private void RecordFrame() {
            this.recordedFrames.Add(new ReplaySkaterState(this.transformsToBeRecorded, this.endTime));
        }


        public void OnGUI() {
            if (this.isRecording) {
                string text = "● Rec";
                Vector2 vector = this.recSkin.CalcSize(new GUIContent(text));
                GUI.Label(new Rect((float)Screen.width - vector.x - 10f, 10f, vector.x, vector.y), text, this.recSkin);
            }
        }


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


        public Transform[] transformsToBeRecorded;


        public bool isRecording;


        public List<ReplaySkaterState> recordedFrames;


        public float endTime;


        private GUIStyle recSkin;


        public float startTime;


        public float MaxRecordedTime;
    }

}
