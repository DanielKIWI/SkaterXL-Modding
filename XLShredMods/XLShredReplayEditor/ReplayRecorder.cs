using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Harmony12;
using System.Collections;

namespace XLShredReplayEditor {

    public class ReplayRecorder : MonoBehaviour {
        public List<Transform> transformsToBeRecorded;

        public List<ReplayRecordedFrame> ClipFrames;
        public List<ReplayRecordedFrame> RecordedFrames;

        public HeadIK PlayerHeadIK;

        public float startTime { get; private set; }
        public float endTime { get; private set; }

        public void CutClip(float newStartTime, float newEndTime) {
            if (newStartTime > startTime) {
                int i = 0;
                while (i < ClipFrames.Count && ClipFrames[i].time < newStartTime) {
                    i++;
                }
                if (i == ClipFrames.Count) {
                    ClipFrames.Clear();
                    startTime = endTime;
                } else {
                    ClipFrames.RemoveRange(0, i);
                    startTime = ClipFrames[0].time;
                }
            }
            if (newEndTime < endTime) {
                int i = 0;
                while (i < ClipFrames.Count && ClipFrames[ClipFrames.Count - 1 - i].time > newEndTime) {
                    i++;
                }
                if (i == ClipFrames.Count) {
                    ClipFrames.Clear();
                    endTime = newStartTime;
                } else {
                    ClipFrames.RemoveRange(ClipFrames.Count - i, i);
                    endTime = ClipFrames[ClipFrames.Count - 1].time;
                }
            }
        }
        public float recordedTime {
            get {
                return endTime - startTime;
            }
        }

        /// <summary>
        /// Used for saving the skater position when opening ReplayEditor
        /// Reason: After cutting the clip the last frame is no longer the frame when opening the Editor
        /// </summary>
        public ReplayRecordedFrame lastFrame;

        public void AddTransformToRecordedList(Transform t) {
            if (transformsToBeRecorded.Contains(t)) {
                Main.modEntry.Logger.Log("Transform " + t.name + " is already being recorded.");
                return;
            }
            transformsToBeRecorded.Add(t);
        }

        public void AddTransformsToRecordedList(IEnumerable<Transform> ts) {
            foreach (Transform t in ts) {
                AddTransformToRecordedList(t);
            }
        }

        public void Awake() {
            PlayerHeadIK = PlayerController.Instance.GetComponentInChildren<HeadIK>();
            StartCoroutine(RecordBailedLoop());
            this.transformsToBeRecorded = new List<Transform>();
            this.RecordedFrames = new List<ReplayRecordedFrame>();

            AddTransformToRecordedList(PlayerController.Instance.transform);

            //Board
            AddTransformToRecordedList(PlayerController.Instance.boardController.boardTransform);
            AddTransformToRecordedList(PlayerController.Instance.boardController.backTruckRigidbody.transform);
            AddTransformToRecordedList(PlayerController.Instance.boardController.frontTruckRigidbody.transform);
            AddTransformToRecordedList(SoundManager.Instance.wheel1);
            AddTransformToRecordedList(SoundManager.Instance.wheel2);
            AddTransformToRecordedList(SoundManager.Instance.wheel3);
            AddTransformToRecordedList(SoundManager.Instance.wheel4);

            //Bones
            AddTransformToRecordedList(PlayerController.Instance.skaterController.skaterTransform);
            AddTransformToRecordedList(PlayerController.Instance.skaterController.skaterRigidbody.transform);

            foreach (object obj in Enum.GetValues(typeof(HumanBodyBones))) {
                HumanBodyBones humanBodyBones = (HumanBodyBones)obj;
                if (humanBodyBones < HumanBodyBones.Hips || humanBodyBones >= HumanBodyBones.LastBone)
                    break;
                Transform boneTransform = PlayerController.Instance.animationController.skaterAnim.GetBoneTransform(humanBodyBones);
                if (boneTransform != null) {
                    AddTransformToRecordedList(boneTransform);
                }
            }
            var ikcTraverse = Traverse.Create(PlayerController.Instance.ikController);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikAnimBoard").Value);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikLeftFootPosition").Value);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikRightFootPosition").Value);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikAnimLeftFootTarget").Value);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikAnimRightFootTarget").Value);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikLeftFootPositionOffset").Value);
            AddTransformToRecordedList(ikcTraverse.Field<Transform>("ikRightFootPositionOffset").Value);

            transformsToBeRecorded.OrderBy(delegate (Transform t) {
                int i = 0;
                while (t.parent != null) {
                    i++;
                    t = t.parent;
                }
                return i;
            });

            this.startTime = 0f;
            this.endTime = 0f;
        }

        public void OnStartReplayEditor() {
            PlayerHeadIK.enabled = false;
            SaveLastFrame();
            ClipFrames = new List<ReplayRecordedFrame>(RecordedFrames);
            startTime = RecordedFrames[0].time;
            endTime = RecordedFrames[ClipFrames.Count - 1].time;
        }

        public void OnExitReplayEditor() {
            PlayerHeadIK.enabled = true;
            startTime = RecordedFrames[0].time;
            endTime = RecordedFrames[RecordedFrames.Count - 1].time;
            ApplyLastFrame();
        }

        public void Destroy() {
            StopAllCoroutines();
            Destroy(this);
        }

        private void ClearRecording() {
            this.ClipFrames.Clear();
            this.startTime = this.endTime;
        }

        public void FixedUpdate() {
            if (ReplayManager.CurrentState != ReplayState.Recording) {
                return;
            }
            this.endTime += Time.fixedDeltaTime;

            //Recording at FixedUpdate when skating.  <->  Physics determinates the Movement
            if (!PlayerController.Instance.respawn.bail.bailed) {
                this.RecordFrame();
            }
            if (this.endTime - startTime > Main.settings.MaxRecordedTime) {
                this.startTime = this.endTime - Main.settings.MaxRecordedTime;
                while (RecordedFrames.Count > 0 && RecordedFrames[0].time < startTime) {
                    RecordedFrames.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// If player has bailed recorded Transforms are changed between FixedUpdate and rendering -> Recording directly after the frame was rendered.  <-> Animations done at Render-Time determinates the Movement
        /// </summary>
        private IEnumerator RecordBailedLoop() {
            while (true) {
                if (ReplayManager.CurrentState != ReplayState.Recording || !PlayerController.Instance.respawn.bail.bailed) {
                    yield return new WaitUntil(() => ReplayManager.CurrentState == ReplayState.Recording && PlayerController.Instance.respawn.bail.bailed);
                }
                yield return new WaitForEndOfFrame();

                this.RecordFrame();
            }
        }
        public void ApplyRecordedFrame(int frame) {
            this.ClipFrames[frame].ApplyTo(this.transformsToBeRecorded);
        }

        public void SaveLastFrame() {
            lastFrame = this.RecordedFrames[RecordedFrames.Count - 1];
        }

        public void ApplyLastFrame() {
            lastFrame.ApplyTo(this.transformsToBeRecorded);
        }

        private void RecordFrame() {
            var transformInfos = new TransformInfo[transformsToBeRecorded.Count];
            for (int i = 0; i < transformsToBeRecorded.Count; i++) {
                Transform t = transformsToBeRecorded[i];
                transformInfos[i] = new TransformInfo(t);
                if (!PlayerController.Instance.respawn.bail.bailed && t == PlayerHeadIK.head)
                    transformInfos[i].rotation = Quaternion.Inverse(t.parent.rotation) * PlayerHeadIK.currentRot.rotation;  //Fixes Head jitter
            }
            ReplayRecordedFrame newFrame = new ReplayRecordedFrame() {
                time = this.endTime,
                transformInfos = transformInfos
            };
            this.RecordedFrames.Add(newFrame);
        }

        public int GetFrameIndex(float time, int lastFrame = 0) {
            if (ClipFrames.Count == 0) {
                Main.modEntry.Logger.Error("RecordedFramse is empty");
                return -1;
            }
            int index;
            if (lastFrame < 0) {
                index = 0;
            } else if (lastFrame > this.ClipFrames.Count - 2) {
                index = this.ClipFrames.Count - 2;
            } else {
                index = lastFrame;
            }
            if (time < this.ClipFrames[index].time) {
                while (index > 0 && this.ClipFrames[index].time > time) {
                    index--;
                }
                return index;
            }
            while (index < this.ClipFrames.Count - 2 && this.ClipFrames[index + 1].time < time) {
                index++;
            }
            return index;
        }

        public void ApplyRecordedTime(int frameIndex, float time) {
            if (this.ClipFrames.Count != 0) {
                if (this.ClipFrames.Count == 1) {
                    this.ClipFrames[0].ApplyTo(this.transformsToBeRecorded);
                    return;
                }
                if (frameIndex > 0 && frameIndex + 1 < this.ClipFrames.Count) {
                    var currentframe = ReplayRecordedFrame.Lerp(ClipFrames[frameIndex], ClipFrames[frameIndex + 1], time);
                    currentframe.ApplyTo(transformsToBeRecorded);
                }
            }
        }

        public void LoadFrames(IEnumerable<ReplayRecordedFrame> frames) {
            ClipFrames = new List<ReplayRecordedFrame>(frames);
            startTime = this.ClipFrames[0].time;
            endTime = this.ClipFrames[this.ClipFrames.Count - 1].time;
            ReplayManager.Instance.playbackTime = startTime;
            ReplayManager.Instance.clipStartTime = startTime;
            ReplayManager.Instance.clipEndTime = endTime;
        }

        void printTransformsToBeRecorded() {
            print("transformsToBeRecorded: ");
            foreach (Transform t in transformsToBeRecorded) {
                print(t.name);
            }
        }
    }
}
