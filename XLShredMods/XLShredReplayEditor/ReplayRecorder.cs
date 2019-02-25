﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Harmony12;

namespace XLShredReplayEditor {

    public class ReplayRecorder : MonoBehaviour {

        public float _startTime;
        public float startTime {
            get { return _startTime; }
            set {
                if (value <= _startTime) return;
                int i = 0;
                while (i < recordedFrames.Count && recordedFrames[i].time < value) {
                    i++;
                }
                if (i == recordedFrames.Count) {
                    recordedFrames.Clear();
                    _startTime = endTime;
                } else {
                    recordedFrames.RemoveRange(0, i);
                    _startTime = recordedFrames[0].time;
                }
            }
        }
        public float _endTime;
        public float endTime {
            get { return _endTime; }
            set {
                if (value >= _endTime) return;
                int i = 0;
                while (i < recordedFrames.Count && recordedFrames[recordedFrames.Count - 1 - i].time > value) {
                    i++;
                }
                if (i == recordedFrames.Count) {
                    recordedFrames.Clear();
                    _endTime = startTime;
                } else {
                    recordedFrames.RemoveRange(recordedFrames.Count - i, i);
                    _endTime = recordedFrames[recordedFrames.Count - 1].time;
                }
            }
        }
        public float recordedTime {
            get {
                return endTime - startTime;
            }
        }
        
        public void Awake() {
            var bcInstance = Traverse.Create(PlayerController.Instance.boardController);
            var wheel1 = bcInstance.Field<Transform>("_wheel1");
            var wheel2 = bcInstance.Field<Transform>("_wheel2");
            var wheel3 = bcInstance.Field<Transform>("_wheel3");
            var wheel4 = bcInstance.Field<Transform>("_wheel4");
            List<Transform> list = new List<Transform>(PlayerController.Instance.respawn.getSpawn);
            list = list.Union(new List<Transform> {
                SoundManager.Instance.wheel1,
                SoundManager.Instance.wheel2,
                SoundManager.Instance.wheel3,
                SoundManager.Instance.wheel4
            }).ToList();
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
            this.recordedFrames = new List<ReplayRecordedFrame>();
            this._startTime = 0f;
            this._endTime = 0f;
            printTransformsToBeRecorded();
        }
        void printTransformsToBeRecorded() {
            print("transformsToBeRecorded: ");
            foreach (Transform t in transformsToBeRecorded) {
                print(t.name);
            }
        }

        private void ClearRecording() {
            this.recordedFrames.Clear();
            this._startTime = this._endTime;
        }

        public void FixedUpdate() {
            if (ReplayManager.CurrentState != ReplayState.RECORDING) {
                return;
            }
            this._endTime += Time.deltaTime;


            this.RecordFrame();
            if (this.endTime - startTime > Main.settings.MaxRecordedTime) {
                this.startTime = this.endTime - Main.settings.MaxRecordedTime;
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
        public void ApplyLastFrame() {
            this.recordedFrames[recordedFrames.Count - 1].ApplyTo(this.transformsToBeRecorded);
        }


        private void RecordFrame() {
            this.recordedFrames.Add(new ReplayRecordedFrame(this.transformsToBeRecorded, this.endTime));
        }


        public void OnGUI() {
            if (ReplayManager.CurrentState == ReplayState.RECORDING && Main.settings.showRecGUI) {
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
                    ReplayRecordedFrame.Lerp(this.recordedFrames[frameIndex], this.recordedFrames[frameIndex + 1], time).ApplyTo(this.transformsToBeRecorded);
                }
            }
        }


        public Transform[] transformsToBeRecorded;


        public List<ReplayRecordedFrame> recordedFrames;




        private GUIStyle recSkin;


    }

}
