using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;

namespace XLShredReplayEditor {
    using Utils;
    [Serializable]
    public class ReplayData {
        public ReplayRecordedFrame[] recordedFrames;
        public SerializableKeyFrame[] cameraKeyFrames;
        public float recordedTime;
        public ReplayData() {
            this.recordedFrames = ReplayManager.Instance.recorder.ClipFrames.Select(f => f.Copy()).ToArray();
            float startTime = ReplayManager.Instance.recorder.startTime;
            float endTime = ReplayManager.Instance.recorder.endTime;
            this.recordedTime = endTime - startTime;
            for (int i = 0; i < this.recordedFrames.Length; i++) {
                this.recordedFrames[i].time -= startTime;
            }

            this.cameraKeyFrames = ReplayManager.Instance.cameraController.keyFrames
                .Where(k => k.time >= startTime && k.time <= endTime)
                .Select(k => SerializableKeyFrame.CreateSerializableKeyFrame(k))
                .ToArray();
            foreach (var keyFrame in cameraKeyFrames) {
                keyFrame.time -= startTime;
            }
        }

        public void Load() {
            Main.modEntry.Logger.Log("Loading ReplayFrames, Recorded Time: " + recordedTime);
            ReplayManager.Instance.recorder.LoadFrames(this.recordedFrames);
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
            string audioPath = path + "\\Audio.wav";
            try {
                BinarySerialization.WriteToBinaryFile<ReplayData>(dataPath, this, false);
            } catch (Exception e) {
                Main.modEntry.Logger.Error("Error saving ReplayData to file at " + path + " Error: " + e.Message);
            }
            ReplayManager.Instance.audioRecorder.WriteTmpStreamToPath(audioPath, ReplayManager.Instance.recorder.startTime, ReplayManager.Instance.recorder.endTime);
        }

        public static IEnumerator LoadFromFile(string path) {
            string audioPath = path + "\\Audio.wav";
            string dataPath = path + "\\Data.replay";
            
            try {
                BinarySerialization.ReadFromBinaryFile<ReplayData>(dataPath).Load();
            } catch (Exception e) {
                Main.modEntry.Logger.Error("Error loading ReplayData from file at " + path + " Error: " + e.Message);
            }
            
            yield return ReplayManager.Instance.audioRecorder.LoadReplayAudio(audioPath);
        }
    }
    [Serializable]
    public class SerializableKeyFrame {
        public CameraMode cameraMode;
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public Vector3Radial radialPos;
        public float fov;
        public float yOffset;
        public float time;

        public static SerializableKeyFrame CreateSerializableKeyFrame(KeyFrame k) {
            if (k is TripodCameraKeyFrame tk)
                return new SerializableKeyFrame(tk);
            if (k is OrbitCameraKeyFrame ok)
                return new SerializableKeyFrame(ok);
            if (k is FreeCameraKeyFrame fk)
                return new SerializableKeyFrame(fk);
            return null;
        }

        public KeyFrame GetKeyFrame(CameraCurve curve) {
            switch (cameraMode) {
                case CameraMode.Free:
                    FreeCameraKeyFrame freeCameraKeyFrame = new FreeCameraKeyFrame(position.Value, rotation.Value, fov, time);
                    freeCameraKeyFrame.AddCurveKeys(curve);
                    return freeCameraKeyFrame;
                case CameraMode.Orbit:
                    OrbitCameraKeyFrame orbitCameraKeyFrame = new OrbitCameraKeyFrame(radialPos, position.Value, rotation.Value, yOffset, fov, time);
                    orbitCameraKeyFrame.AddCurveKeys(curve);
                    return orbitCameraKeyFrame;
                case CameraMode.Tripod:
                    TripodCameraKeyFrame tripodCameraKeyFrame = new TripodCameraKeyFrame(position.Value, rotation.Value, yOffset, fov, time);
                    tripodCameraKeyFrame.AddCurveKeys(curve);
                    return tripodCameraKeyFrame;
                default:
                    throw new Exception("Unknown cameraMode: " + cameraMode);
            }
        }

        public SerializableKeyFrame(FreeCameraKeyFrame fk) {
            this.cameraMode = CameraMode.Free;
            this.position = new SerializableVector3(fk.position);
            this.rotation = new SerializableQuaternion(fk.rotation);
            this.fov = fk.fov;
            this.time = fk.time;
        }

        public SerializableKeyFrame(TripodCameraKeyFrame tk) {
            this.cameraMode = CameraMode.Tripod;
            this.position = new SerializableVector3(tk.position);
            this.rotation = new SerializableQuaternion(tk.rotation);
            this.yOffset = tk.focusOffsetY;
            this.fov = tk.fov;
            this.time = tk.time;
        }

        public SerializableKeyFrame(OrbitCameraKeyFrame ok) {
            this.cameraMode = CameraMode.Orbit;
            this.position = new SerializableVector3(ok.position);
            this.rotation = new SerializableQuaternion(ok.rotation);
            this.yOffset = ok.focusOffsetY;
            this.radialPos = ok.radialPos;
            this.fov = ok.fov;
            this.time = ok.time;
        }
    }
}
