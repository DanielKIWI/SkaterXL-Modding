using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {
    [Serializable]
    public class KeyFrame {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
        public float fov;

        public virtual void Update(Transform cameraTransform, float t) {
            time = t;
        }
        public virtual void ApplyTo(Camera camera) { }
        public static CameraCurveResult Evaluate(float time, CameraCurve cameraCurve) { return null; }
        public virtual void AddKeyframes(CameraCurve cameraCurve) { }
    }

    public abstract class KeyFrameWithYOffset : KeyFrame {
        public float focusOffsetY;
    }
    
    #region KeyFrame Implementations
    [Serializable]
    public class FreeCameraKeyFrame : KeyFrame {

        public FreeCameraKeyFrame(Transform cameraTransform, float fov, float time, CameraCurve cameraCurve) {
            position = cameraTransform.position;
            rotation = cameraTransform.rotation;
            this.time = time;
            this.fov = fov;
            AddKeyframes(cameraCurve);
        }

        public FreeCameraKeyFrame(Vector3 pos, Quaternion rot, float fov, float time, CameraCurve cameraCurve) {
            position = pos;
            rotation = rot;
            this.time = time;
            this.fov = fov;
            AddKeyframes(cameraCurve);
        }

        private float CalcFocusYOffset() {
            Vector3 camToPlayer = PlayerController.Instance.skaterController.skaterTransform.position - position;
            Quaternion camToPlayerLookRotation = Quaternion.LookRotation(camToPlayer, Vector3.up);
            Quaternion rotOffset = Quaternion.FromToRotation(camToPlayerLookRotation * Vector3.up, rotation * Vector3.up);
            Vector3 n = Vector3.Cross(camToPlayer, Vector3.up);
            Vector3 alignedLookVec = Vector3.ProjectOnPlane(position, (rotOffset * camToPlayer)).normalized;

            Vector3 lookVec = alignedLookVec * (camToPlayer.x / (alignedLookVec.x - (position - PlayerController.Instance.skaterController.skaterTransform.position).normalized.x));
            return Vector3.Project(lookVec, Vector3.up).magnitude;
        }

        public override void AddKeyframes(CameraCurve cameraCurve) {
            Quaternion camToPlayerLookRotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - position, Vector3.up);

            cameraCurve.positionCurve.InsertCurveKey(position, time);
            cameraCurve.orientationCurve.InsertCurveKey(rotation, time);

            cameraCurve.radiusCurve.InsertCurveKey((position - PlayerController.Instance.skaterController.skaterTransform.position).magnitude, time);
            cameraCurve.focusYOffsetCurve.InsertCurveKey(CalcFocusYOffset(), time);
            cameraCurve.fovCurve.InsertCurveKey(fov, time);

            cameraCurve.freeCamCurve.InsertCurveKey(1f, time);
            cameraCurve.orbitCamCurve.InsertCurveKey(0f, time);
            cameraCurve.tripodCamCurve.InsertCurveKey(0f, time);

            cameraCurve.CalculateCurveControlPoints();
        }

        public override void ApplyTo(Camera camera) {
            camera.transform.position = position;
            camera.transform.rotation = rotation;
            camera.fieldOfView = fov;
        }

        public new static CameraCurveResult Evaluate(float time, CameraCurve cameraCurve) {
            Vector3 pos = cameraCurve.positionCurve.Evaluate(time);
            Quaternion rot = cameraCurve.orientationCurve.Evaluate(time);
            float fov = cameraCurve.fovCurve.Evaluate(time);

            return new CameraCurveResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }
    }
    [Serializable]
    public class OrbitCameraKeyFrame : KeyFrameWithYOffset {
        public Vector3Radial radialPos;
        public float radius {
            get {
                return radialPos.cartesianCoords.magnitude;
            }
        }
        public Vector3 focusLocation;

        public OrbitCameraKeyFrame(Vector3Radial radialPos, float yOffset, float fov, float t, CameraCurve cameraCurve) {
            this.radialPos = radialPos;
            position = PlayerController.Instance.skaterController.skaterTransform.position + Vector3.up * focusOffsetY + radialPos.cartesianCoords;
            rotation = Quaternion.LookRotation(-radialPos.cartesianCoords, Vector3.up);
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;
            AddKeyframes(cameraCurve);
        }

        public OrbitCameraKeyFrame(Vector3Radial radialPos, Vector3 pos, Quaternion rot, float yOffset, float fov, float t, CameraCurve cameraCurve) {
            this.radialPos = radialPos;
            position = PlayerController.Instance.skaterController.skaterTransform.position + Vector3.up * focusOffsetY + radialPos.cartesianCoords;
            rotation = Quaternion.LookRotation(-radialPos.cartesianCoords, Vector3.up);
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;
            AddKeyframes(cameraCurve);
        }

        public override void AddKeyframes(CameraCurve cameraCurve) {
            Vector3 calculatedPosition = PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up + rotation * -Vector3.forward * radius;

            cameraCurve.positionCurve.InsertCurveKey(calculatedPosition, time);
            cameraCurve.orientationCurve.InsertCurveKey(rotation, time);

            cameraCurve.radiusCurve.InsertCurveKey(radius, time);
            cameraCurve.focusYOffsetCurve.InsertCurveKey(focusOffsetY, time);
            cameraCurve.fovCurve.InsertCurveKey(fov, time);

            cameraCurve.freeCamCurve.InsertCurveKey(0f, time);
            cameraCurve.orbitCamCurve.InsertCurveKey(1f, time);
            cameraCurve.tripodCamCurve.InsertCurveKey(0f, time);

            cameraCurve.CalculateCurveControlPoints();
        }

        public override void ApplyTo(Camera camera) {
            camera.transform.position = PlayerController.Instance.skaterController.skaterTransform.position + Vector3.up * focusOffsetY + radialPos.cartesianCoords;
            camera.transform.rotation = Quaternion.LookRotation(-radialPos.cartesianCoords, Vector3.up);
            camera.fieldOfView = fov;
        }

        public override void Update(Transform cameraTransform, float t) {
            position = PlayerController.Instance.skaterController.skaterTransform.position + Vector3.up * focusOffsetY + radialPos.cartesianCoords;
            base.Update(cameraTransform, t);
        }

        public new static CameraCurveResult Evaluate(float time, CameraCurve cameraCurve) {
            Quaternion rot = cameraCurve.orientationCurve.Evaluate(time);
            Vector3 pos =    PlayerController.Instance.skaterController.skaterTransform.position
                                            + cameraCurve.focusYOffsetCurve.Evaluate(time) * Vector3.up
                                            + rot * -Vector3.forward * cameraCurve.radiusCurve.Evaluate(time);

            float fov = cameraCurve.fovCurve.Evaluate(time);

            return new CameraCurveResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }
    }

    [Serializable]
    public class TripodCameraKeyFrame : KeyFrameWithYOffset {

        public TripodCameraKeyFrame(Transform cameraTransform, float yOffset, float fov, float t, CameraCurve cameraCurve) {
            position = cameraTransform.position;
            rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up - position, Vector3.up);
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;

            AddKeyframes(cameraCurve);
        }

        public TripodCameraKeyFrame(Vector3 pos, Quaternion rot, float yOffset, float fov, float t, CameraCurve cameraCurve) {
            position = pos;
            rotation = rot;
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;

            AddKeyframes(cameraCurve);
        }

        public override void AddKeyframes(CameraCurve cameraCurve) {
            cameraCurve.positionCurve.InsertCurveKey(position, time);
            cameraCurve.orientationCurve.InsertCurveKey(rotation, time);

            cameraCurve.radiusCurve.InsertCurveKey((position - PlayerController.Instance.skaterController.skaterTransform.position).magnitude, time);
            cameraCurve.focusYOffsetCurve.InsertCurveKey(focusOffsetY, time);
            cameraCurve.fovCurve.InsertCurveKey(fov, time);

            cameraCurve.freeCamCurve.InsertCurveKey(0f, time);
            cameraCurve.orbitCamCurve.InsertCurveKey(0f, time);
            cameraCurve.tripodCamCurve.InsertCurveKey(1f, time);

            cameraCurve.CalculateCurveControlPoints();
        }

        public override void ApplyTo(Camera camera) {
            camera.transform.position = position;
            camera.transform.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up - position, Vector3.up);
            camera.fieldOfView = fov;
        }

        public override void Update(Transform cameraTransform, float t) {
            rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up - position, Vector3.up);
            base.Update(cameraTransform, t);
        }

        public new static CameraCurveResult Evaluate(float time, CameraCurve cameraCurve) {;
            Vector3 pos = cameraCurve.positionCurve.Evaluate(time);
            Quaternion rot = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + cameraCurve.focusYOffsetCurve.Evaluate(time) * Vector3.up - pos, Vector3.up);
            float fov = cameraCurve.fovCurve.Evaluate(time);

            return new CameraCurveResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }
    }
    #endregion
}
