using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {
    [Serializable]
    public class KeyStone {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
        public float fov;

        public static CameraCurveResult Evaluate(float time, CameraCurve cameraCurve) { return null; }
        public virtual void AddKeyframes(CameraCurve cameraCurve) { }
    }

    public abstract class KeyStoneWithYOffset : KeyStone {
        public float focusOffsetY;
    }

    #region KeyStone Implementations
    [Serializable]
    public class FreeCameraKeyStone : KeyStone {

        public FreeCameraKeyStone(Transform cameraTransform, float fov, float time, CameraCurve cameraCurve) {
            position = cameraTransform.position;
            rotation = cameraTransform.rotation;
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
    public class OrbitCameraKeyStone : KeyStoneWithYOffset {
        public float radius;
        public Vector3 focusLocation;

        public OrbitCameraKeyStone(Vector3Radial radialPos, float yOffset, float fov, float t, CameraCurve cameraCurve) {
            this.radius = radialPos.cartesianCoords.magnitude;
            position = PlayerController.Instance.skaterController.skaterTransform.position; // not really necessary
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
    public class TripodCameraKeyStone : KeyStoneWithYOffset {
        public TripodCameraKeyStone(Transform cameraTransform, float yOffset, float fov, float t, CameraCurve cameraCurve) {
            position = cameraTransform.position;
            rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up - position, Vector3.up);
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
