﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {
    [Serializable]
    public class KeyStone {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
        public float fov;
        private Vector3 velocity = Vector3.zero;

        public static CameraCurveResult Evaluate(float time) { return null; }
        public virtual void AddKeyframes() { }
    }

    public abstract class KeyStoneWithYOffset : KeyStone {
        public float focusOffsetY;
        public Quaternion baseRotation;
    }

    #region KeyStone Implementations
    [Serializable]
    public class FreeCameraKeyStone : KeyStone {

        public FreeCameraKeyStone(Transform cameraTransform, float fov, float time) {
            position = cameraTransform.position;
            rotation = cameraTransform.rotation;
            this.time = time;
            this.fov = fov;
            AddKeyframes();
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

        public override void AddKeyframes() {
            Quaternion camToPlayerLookRotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - position, Vector3.up);

            CameraCurve.positionCurve.InsertCurveKey(position, time);
            CameraCurve.orientationCurve.InsertCurveKey(rotation, time);
            CameraCurve.lookRotCurve.InsertCurveKey(camToPlayerLookRotation, time);
            CameraCurve.fovCurve.InsertCurveKey(fov, time);
            CameraCurve.radiusCurve.InsertCurveKey((position - PlayerController.Instance.skaterController.skaterTransform.position).magnitude, time);
            CameraCurve.focusYOffsetCurve.InsertCurveKey(CalcFocusYOffset(), time);

            CameraCurve.freeCamCurve.InsertCurveKey(1f, time);
            CameraCurve.orbitCamCurve.InsertCurveKey(0f, time);
            CameraCurve.tripodCamCurve.InsertCurveKey(0f, time);

            CameraCurve.CalculateCurveControlPoints();
        }

        public new static CameraCurveResult Evaluate(float time) {
            Vector3 pos = CameraCurve.positionCurve.Evaluate(time);
            Quaternion rot = CameraCurve.orientationCurve.Evaluate(time);
            float fov = CameraCurve.fovCurve.Evaluate(time);

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

        public OrbitCameraKeyStone(Vector3Radial radialPos, float yOffset, float fov, float t) {
            this.radius = radialPos.CartesianCoords.magnitude;
            position = PlayerController.Instance.skaterController.skaterTransform.position; // not really necessary
            rotation = Quaternion.LookRotation(-radialPos.CartesianCoords, Vector3.up);
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;
            AddKeyframes();
        }

        public override void AddKeyframes() {
            Vector3 calculatedPosition = PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up + rotation * -Vector3.forward * radius;

            CameraCurve.positionCurve.InsertCurveKey(calculatedPosition, time);
            CameraCurve.orientationCurve.InsertCurveKey(rotation, time);
            CameraCurve.lookRotCurve.InsertCurveKey(rotation, time);
            CameraCurve.fovCurve.InsertCurveKey(fov, time);
            CameraCurve.radiusCurve.InsertCurveKey(radius, time);
            CameraCurve.focusYOffsetCurve.InsertCurveKey(focusOffsetY, time);

            CameraCurve.freeCamCurve.InsertCurveKey(0f, time);
            CameraCurve.orbitCamCurve.InsertCurveKey(1f, time);
            CameraCurve.tripodCamCurve.InsertCurveKey(0f, time);

            CameraCurve.CalculateCurveControlPoints();
        }

        public new static CameraCurveResult Evaluate(float time) {
            Quaternion rot = CameraCurve.orientationCurve.Evaluate(time);
            Vector3 pos =    PlayerController.Instance.skaterController.skaterTransform.position
                                            + CameraCurve.focusYOffsetCurve.Evaluate(time) * Vector3.up
                                            + rot * -Vector3.forward * CameraCurve.radiusCurve.Evaluate(time);

            float fov = CameraCurve.fovCurve.Evaluate(time);

            return new CameraCurveResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }

        /*
        public override void ApplyTo(Transform t) {
            t.position = PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up + this.radialPos.cartesianCoords;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            t.GetComponent<Camera>().fieldOfView = this.fov;
        }

        public static OrbitCameraKeyStone Lerp(OrbitCameraKeyStone a, OrbitCameraKeyStone b, float time) {
            float t = (time - a.Time) / (b.Time - a.Time);
            return new OrbitCameraKeyStone(Vector3Radial.Lerp(a.radialPos, b.radialPos, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.focusOffsetY, b.focusOffsetY, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.fov, b.fov, Mathf.SmoothStep(0f, 1f, t)), time);
        }

        public new static KeyStoneResult Evaluate(float time) {



            float fov = KeyStoneCurves.animationCurves[CurveKeyType.Fov].Evaluate(time);

            return new KeyStoneResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }*/
    }

    [Serializable]
    public class TripodCameraKeyStone : KeyStoneWithYOffset {
        public TripodCameraKeyStone(Transform cameraTransform, float yOffset, float fov, float t) {
            position = cameraTransform.position;
            rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - position, Vector3.up);
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;

            AddKeyframes();
        }

        public override void AddKeyframes() {
            Quaternion calculatedRot = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up - position, Vector3.up);

            CameraCurve.positionCurve.InsertCurveKey(position, time);
            CameraCurve.orientationCurve.InsertCurveKey(calculatedRot, time);
            CameraCurve.lookRotCurve.InsertCurveKey(rotation, time);
            CameraCurve.fovCurve.InsertCurveKey(fov, time);
            CameraCurve.radiusCurve.InsertCurveKey((position - PlayerController.Instance.skaterController.skaterTransform.position).magnitude, time);
            CameraCurve.focusYOffsetCurve.InsertCurveKey(focusOffsetY, time);

            CameraCurve.freeCamCurve.InsertCurveKey(0f, time);
            CameraCurve.orbitCamCurve.InsertCurveKey(0f, time);
            CameraCurve.tripodCamCurve.InsertCurveKey(1f, time);

            CameraCurve.CalculateCurveControlPoints();
        }
        /*
        public override void ApplyTo(Transform t) {
            t.position = position;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            t.GetComponent<Camera>().fieldOfView = this.fov;
        }

        public static TripodCameraKeyStone Lerp(KeyStone a, KeyStone b, float time) {
            float t = (time - a.Time) / (b.Time - a.Time);
            float yOffset = 0f;
            KeyStoneWithYOffset ayo = a as KeyStoneWithYOffset;
            KeyStoneWithYOffset byo = b as KeyStoneWithYOffset;
            if (ayo != null) {
                if (byo != null) {
                    yOffset = Mathf.Lerp(ayo.focusOffsetY, byo.focusOffsetY, t);
                } else {
                    yOffset = ayo.focusOffsetY;
                }
            } else if (byo != null) {
                yOffset = byo.focusOffsetY;
            }
            return new TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.fov, b.fov, Mathf.SmoothStep(0f, 1f, t)), yOffset, time);
        }*/
    }
    #endregion
}
