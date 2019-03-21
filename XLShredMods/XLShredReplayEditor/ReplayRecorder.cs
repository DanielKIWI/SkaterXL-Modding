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

        public void AddTransformToRecordedList(Transform t) {
            if (transformsToBeRecorded.Contains(t)) {
                Debug.Log("Transform " + t.name + " is already being recorded.");
                return;
            }
            transformsToBeRecorded.Add(t);
        }

        public void RecordTransforms(IEnumerable<Transform> ts) {
            foreach (Transform t in ts) {
                AddTransformToRecordedList(t);
            }
        }

        public void Awake() {
            StartCoroutine(RecordBailedLoop());
            this.transformsToBeRecorded = new List<Transform>();
            this.recordedFrames = new List<ReplayRecordedFrame>();

            //RecordTransforms(PlayerController.Instance.transform.GetComponentsInChildren<Transform>());
            //Board
            AddTransformToRecordedList(PlayerController.Instance.boardController.boardTransform);
            AddTransformToRecordedList(PlayerController.Instance.boardController.backTruckRigidbody.transform);
            AddTransformToRecordedList(PlayerController.Instance.boardController.frontTruckRigidbody.transform);
            AddTransformToRecordedList(SoundManager.Instance.wheel1);
            AddTransformToRecordedList(SoundManager.Instance.wheel2);
            AddTransformToRecordedList(SoundManager.Instance.wheel3);
            AddTransformToRecordedList(SoundManager.Instance.wheel4);

            ////Bones
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

            var bailTraverse = Traverse.Create(PlayerController.Instance.respawn.bail);
            RecordTransforms(bailTraverse.Field<RootMotion.Dynamics.PuppetMaster>("_puppetMaster").Value.GetComponentsInChildren<Transform>());
            //RecordTransform(ikcInstance.Field<Transform>());
            this._startTime = 0f;
            this._endTime = 0f;
            //printTransformsToBeRecorded();
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
            this._endTime += Time.fixedDeltaTime;
            
            //Recording at FixedUpdate when skating.  <->  Physics determinates the Movement
            if (!PlayerController.Instance.respawn.bail.bailed) {
                this.RecordFrame();
            }
            if (this.endTime - startTime > Main.settings.MaxRecordedTime) {
                this.startTime = this.endTime - Main.settings.MaxRecordedTime;
            }
        }

        //If player has bailed recorded Transforms are changed between FixedUpdate and rendering -> Recording directly after the frame was rendered.  <-> Animations done at Render-Time determinates the Movement
        private IEnumerator RecordBailedLoop() {
            while (true) {
                if (ReplayManager.CurrentState != ReplayState.RECORDING || !PlayerController.Instance.respawn.bail.bailed) {
                    yield return new WaitUntil(() => ReplayManager.CurrentState == ReplayState.RECORDING && PlayerController.Instance.respawn.bail.bailed);
                }
                yield return new WaitForEndOfFrame();

                this.RecordFrame();
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
                this.recSkin = new GUIStyle(GUI.skin.label);
                this.recSkin.fontSize = 20;
                this.recSkin.normal.textColor = Color.red;
                string text = "● Rec";
                Vector2 vector = this.recSkin.CalcSize(new GUIContent(text));
                GUI.Label(new Rect((float)Screen.width - vector.x - 10f, 10f, vector.x, vector.y), text, this.recSkin);
            }
        }


        public int GetFrameIndex(float time, int lastFrame = 0) {
            if (recordedFrames.Count == 0) {
                Main.modEntry.Logger.Error("RecordedFramse is empty");
                return -1;
            }
            int index;
            if (lastFrame < 0) {
                index = 0;
            } else if (lastFrame > this.recordedFrames.Count - 2) {
                index = this.recordedFrames.Count - 2;
            } else {
                index = lastFrame;
            }
            if (time < this.recordedFrames[lastFrame].time) {
                while (index > 0 && this.recordedFrames[index].time > time) {
                    index--;
                }
                return index;
            }
            while (index < this.recordedFrames.Count - 2 && this.recordedFrames[index + 1].time < time) {
                index++;
            }
            return index;
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

        public void LoadFrames(List<ReplayRecordedFrame> frames) {
            recordedFrames = frames;
            _startTime =
            _startTime = this.recordedFrames[0].time;
            _endTime = this.recordedFrames[this.recordedFrames.Count - 1].time;
        }


        public List<Transform> transformsToBeRecorded;


        public List<ReplayRecordedFrame> recordedFrames;




        private GUIStyle recSkin;


    }

}
